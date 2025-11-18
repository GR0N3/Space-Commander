# Sistema Centralizado de Vida (Life Manager)

Este documento explica cómo usar el sistema centralizado de vida implementado con los patrones **Type Object** y **Facade** en Unity.

## 📋 Concepto

El sistema de vida centralizado permite definir variaciones de entidades (jugador, enemigos, etc.) como **datos** (ScriptableObjects) en lugar de crear múltiples clases. Esto hace que el sistema sea:

- ✅ **Flexible**: Agregar nuevos tipos de entidades es tan simple como crear un nuevo ScriptableObject
- ✅ **Escalable**: No necesitas modificar código para agregar nuevos tipos
- ✅ **Mantenible**: Todas las entidades comparten la misma lógica base
- ✅ **Centralizado**: La muerte se maneja a través de una Facade que coordina todos los subsistemas

## 🎯 Componentes Principales

### 1. `LifeType` (ScriptableObject)

Define los datos de vida de una entidad. Crea uno desde el menú: **Create > Type Object > LifeType**

**Atributos principales:**
- `entityName`: Nombre de la entidad
- `maxHealth`: Vida máxima
- `hasShield`: Si puede tener escudo
- `shieldDuration`: Duración del escudo en segundos
- `shieldHealth`: Vida del escudo
- `deathEffectPrefab`: Prefab de explosión/efecto visual
- `deathSound`: Sonido de muerte
- `pointsOnDeath`: Puntos que otorga al morir (solo enemigos)

### 2. `EntityLife` (Componente)

Componente base que gestiona la vida de cualquier entidad (jugador o enemigo). Usa `LifeType` para inicializar y `LifeFacade` para manejar la muerte.

### 3. `LifeFacade` (Facade)

Coordina todos los subsistemas cuando una entidad muere:
- Reproduce sonido de explosión
- Activa efectos visuales
- Suma puntos al marcador (solo enemigos)
- Destruye/desactiva la entidad

### 4. `PlayerHealth` y `EnemyLife`

Componentes específicos que envuelven `EntityLife` para mantener compatibilidad con código existente.

## 🚀 Uso Básico

### Crear un Tipo de Vida Nuevo

1. **Crear el ScriptableObject:**
   - Click derecho en el Project
   - **Create > Type Object > LifeType**
   - Nómbralo (ej: "LifeType_Player", "LifeType_BasicEnemy")

2. **Configurar el LifeType:**
   - Abre el ScriptableObject creado
   - Configura `maxHealth` (ej: 100 para jugador, 50 para enemigo básico)
   - Activa `hasShield` si quieres que tenga escudo
   - Asigna `deathEffectPrefab` y `deathSound`
   - Para enemigos, configura `pointsOnDeath`
   - Guarda

3. **Asignar al GameObject:**
   - Selecciona el GameObject del jugador/enemigo
   - Agrega el componente `EntityLife` (o `PlayerHealth`/`EnemyLife`)
   - Arrastra el ScriptableObject al campo `lifeType` (o `playerLifeType`/`enemyLifeType`)

### Ejemplo: LifeType para Jugador

```csharp
// En el ScriptableObject "LifeType_Player":
entityName = "Player"
maxHealth = 100
hasShield = true
shieldDuration = 5.0
shieldHealth = 50
pointsOnDeath = 0
```

### Ejemplo: LifeType para Enemigo Básico

```csharp
// En el ScriptableObject "LifeType_BasicEnemy":
entityName = "Basic Enemy"
maxHealth = 50
hasShield = false
pointsOnDeath = 100
```

### Ejemplo: LifeType para Enemigo Jefe

```csharp
// En el ScriptableObject "LifeType_BossEnemy":
entityName = "Boss Enemy"
maxHealth = 500
hasShield = true
shieldDuration = 10.0
shieldHealth = 100
pointsOnDeath = 1000
```

## 💻 Código de Ejemplo

### Aplicar Daño a una Entidad

```csharp
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    public int damageAmount = 10;
    
    void OnTriggerEnter(Collider other)
    {
        EntityLife entityLife = other.GetComponent<EntityLife>();
        if (entityLife != null)
        {
            entityLife.TakeDamage(damageAmount);
        }
    }
}
```

### Activar Escudo Programáticamente

```csharp
EntityLife playerLife = GetComponent<EntityLife>();
if (playerLife != null && playerLife.lifeType.hasShield)
{
    playerLife.ActivateShield();
}
```

### Verificar Estado de Vida

```csharp
EntityLife entityLife = GetComponent<EntityLife>();

if (entityLife != null)
{
    Debug.Log($"Vida actual: {entityLife.CurrentHealth}/{entityLife.MaxHealth}");
    Debug.Log($"Escudo activo: {entityLife.IsShieldActive}");
    Debug.Log($"Vida del escudo: {entityLife.CurrentShieldHealth}");
}
```

### Escuchar Cambios de Vida (UI)

```csharp
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    void OnHealthChanged(HealthChangedData data)
    {
        // Actualizar barra de vida
        float healthPercentage = (float)data.currentHealth / data.maxHealth;
        UpdateHealthBar(healthPercentage);
    }
    
    void OnShieldActivated()
    {
        // Mostrar indicador de escudo
        ShowShieldIndicator();
    }
    
    void OnShieldBroken()
    {
        // Ocultar indicador de escudo
        HideShieldIndicator();
    }
    
    void OnEntityDeath()
    {
        // Mostrar pantalla de muerte o game over
        ShowDeathScreen();
    }
    
    private void UpdateHealthBar(float percentage) { /* ... */ }
    private void ShowShieldIndicator() { /* ... */ }
    private void HideShieldIndicator() { /* ... */ }
    private void ShowDeathScreen() { /* ... */ }
}
```

## 🔄 Integración con Sistema de Escudo

El sistema de vida integra automáticamente con el sistema de escudo:

1. **Escudo desde Power-Up**: Cuando el jugador recoge un power-up de escudo, se activa automáticamente si `LifeType.hasShield` es true.

2. **Escudo Absorbe Daño**: Cuando una entidad recibe daño:
   - Si tiene escudo activo, el escudo absorbe el daño primero
   - Si el escudo se rompe, el daño restante se aplica a la vida
   - Si no hay escudo, el daño se aplica directamente a la vida

## 📝 Configuración de LifeFacade

1. **Crear GameObject LifeFacade:**
   - Crea un GameObject vacío en la escena
   - Nómbralo "LifeFacade"
   - Agrega el componente `LifeFacade`

2. **Configurar LifeFacade:**
   - Asigna un `AudioSource` (opcional, se buscará automáticamente)
   - Asigna un `defaultExplosionPrefab` (se usa si LifeType no tiene uno)
   - Asigna un `scoreManager` (GameObject que maneja la puntuación)
   - Configura `useGlobalAudio` según prefieras

## 🎨 Flujo de Muerte

Cuando una entidad muere:

1. `EntityLife` detecta que `currentHealth <= 0`
2. Llama a `LifeFacade.HandleDeath()`
3. `LifeFacade` coordina:
   - Reproduce `deathSound` del `LifeType`
   - Spawnea `deathEffectPrefab` en la posición
   - Suma `pointsOnDeath` al marcador (si > 0)
   - Destruye/desactiva la entidad

## 🐛 Troubleshooting

**Problema**: La entidad no muere
- ✅ Verifica que `LifeType` esté asignado en el Inspector
- ✅ Verifica que `LifeFacade` exista en la escena
- ✅ Verifica que `maxHealth` sea mayor que 0

**Problema**: No se reproducen sonidos
- ✅ Verifica que `LifeType.deathSound` esté asignado
- ✅ Verifica que `LifeFacade` tenga un `AudioSource` o que `useGlobalAudio` esté activo

**Problema**: No se suman puntos
- ✅ Verifica que `LifeType.pointsOnDeath` sea mayor que 0
- ✅ Verifica que `LifeFacade.scoreManager` esté asignado o que exista un GameObject con tag "ScoreManager"

**Problema**: El escudo no funciona
- ✅ Verifica que `LifeType.hasShield` esté activo
- ✅ Verifica que `shieldHealth` sea mayor que 0

## 📚 Referencias

- Patrón Type Object: [Game Programming Patterns - Type Object](https://gameprogrammingpatterns.com/type-object.html)
- Patrón Facade: [Game Programming Patterns - Facade](https://gameprogrammingpatterns.com/facade.html)
- ScriptableObjects en Unity: [Unity Documentation - ScriptableObject](https://docs.unity3d.com/Manual/class-ScriptableObject.html)

