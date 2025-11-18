# Guía de Configuración - Sistema de Vida y Daño

Esta guía explica paso a paso cómo configurar el sistema de vida para que las balas hagan daño y destruyan los GameObjects cuando se queden sin vida.

## 📋 Configuración Paso a Paso

### 1. Configurar LifeFacade (Sistema Centralizado de Muerte)

1. **Crear GameObject LifeFacade:**
   - En la jerarquía, click derecho → Create Empty
   - Nómbralo "LifeFacade"
   - Agrega el componente `LifeFacade`

2. **Configurar LifeFacade:**
   - **AudioSource** (opcional): Asigna un AudioSource si quieres sonidos globales
   - **defaultExplosionPrefab**: Asigna un prefab de explosión por defecto
   - **scoreManager**: Asigna el GameObject que maneja la puntuación (debe tener un método `AddScore(int)`)
   - **useGlobalAudio**: Actívalo si quieres sonidos globales, desactívalo para sonidos 3D

### 2. Configurar el Jugador

#### Paso 1: Crear LifeType para el Jugador

1. Click derecho en el Project → **Create > Type Object > LifeType**
2. Nómbralo "LifeType_Player"
3. Configura:
   - `entityName = "Player"`
   - `maxHealth = 100` (o el valor que prefieras)
   - `hasShield = true` (si quieres que pueda tener escudo)
   - `shieldHealth = 100` (resistencia del escudo cuando se activa desde LifeType)
   - `pointsOnDeath = 0` (el jugador no otorga puntos)
   - `deathEffectPrefab`: Asigna un prefab de explosión
   - `deathSound`: Asigna un AudioClip de muerte

#### Paso 2: Configurar PlayerHealth

1. Selecciona el GameObject del jugador
2. Verifica que tenga el componente `PlayerHealth`
3. Si no lo tiene, agrégalo: Add Component → `PlayerHealth`
4. En el Inspector de `PlayerHealth`:
   - Arrastra "LifeType_Player" al campo `playerLifeType`
5. El componente `EntityLife` se agregará automáticamente (requerido por `PlayerHealth`)

#### Paso 3: Configurar PlayerPowerUpManager

1. Verifica que el jugador tenga `PlayerPowerUpManager`
2. Asigna los GameObjects:
   - `Shell_1`: GameObject visual del escudo nivel 1
   - `Shell_2`: GameObject visual del escudo nivel 2
3. Los `ShieldComponent` se agregarán automáticamente a Shell_1 y Shell_2

### 3. Configurar Enemigos

#### Paso 1: Crear LifeType para Enemigos

1. Click derecho en el Project → **Create > Type Object > LifeType**
2. Nómbralo "LifeType_BasicEnemy" (o el nombre que prefieras)
3. Configura:
   - `entityName = "Basic Enemy"`
   - `maxHealth = 50` (o el valor que prefieras)
   - `hasShield = false` (los enemigos normalmente no tienen escudo)
   - `pointsOnDeath = 100` (puntos que otorga al morir)
   - `deathEffectPrefab`: Asigna un prefab de explosión
   - `deathSound`: Asigna un AudioClip de explosión

#### Paso 2: Configurar EnemyLife

1. Selecciona el GameObject del enemigo
2. Agrega el componente `EnemyLife`: Add Component → `EnemyLife`
3. En el Inspector de `EnemyLife`:
   - Arrastra "LifeType_BasicEnemy" al campo `enemyLifeType`
4. El componente `EntityLife` se agregará automáticamente

### 4. Configurar Balas para Hacer Daño

#### Paso 1: Verificar BulletData

1. Abre el ScriptableObject `PlayerBulletData` (o crea uno nuevo)
2. Verifica que `damage` esté configurado (ej: `damage = 10`)
3. Verifica que `ownerTag = "Player"`

#### Paso 2: Verificar BulletData de Enemigos

1. Abre el ScriptableObject `EnemyBulletData` (o crea uno nuevo)
2. Verifica que `damage` esté configurado (ej: `damage = 5`)
3. Verifica que `ownerTag = "Enemy"`

#### Paso 3: Verificar Bullet Component

El componente `Bullet` ya está configurado para:
- Detectar colisiones con jugador/enemigos
- Aplicar daño usando `EntityLife.TakeDamage()`
- Desactivarse después de impactar

**No necesitas hacer nada más aquí**, el sistema ya está integrado.

### 5. Configurar Sistema de Escudo con Resistencia

#### Paso 1: Crear PowerUpType para Escudo Nivel 1

1. Click derecho → **Create > Type Object > PowerUpType**
2. Nómbralo "PowerUp_Shield1"
3. Configura:
   - `powerUpName = "Escudo Nivel 1"`
   - `shieldActive = true`
   - `shieldResistance = 100` (resistencia del escudo nivel 1)
   - `shieldLevel = 1`

#### Paso 2: Crear PowerUpType para Escudo Nivel 2

1. Click derecho → **Create > Type Object > PowerUpType**
2. Nómbralo "PowerUp_Shield2"
3. Configura:
   - `powerUpName = "Escudo Nivel 2"`
   - `shieldActive = true`
   - `shieldResistance = 200` (resistencia del escudo nivel 2)
   - `shieldLevel = 2`

#### Paso 3: Asignar a Power-Ups

1. Selecciona el GameObject del power-up de escudo
2. En el componente `PowerUp`, arrastra el `PowerUpType` correspondiente

## 🔄 Flujo de Daño y Muerte

### Cuando una Bala Impacta:

1. **Bala detecta colisión** (`Bullet.OnTriggerEnter`)
2. **Verifica el tag** (Player o Enemy)
3. **Busca `EntityLife`** en el GameObject impactado
4. **Llama `EntityLife.TakeDamage(damage)`**

### Cuando una Entidad Recibe Daño:

1. **EntityLife.TakeDamage()** se ejecuta
2. **Verifica si hay escudo activo:**
   - Si hay escudo: reduce resistencia del escudo primero
   - Si el escudo se rompe: aplica daño restante a la vida
   - Si no hay escudo: aplica daño directamente a la vida
3. **Si la vida llega a 0:**
   - Llama a `Die()`
   - `Die()` llama a `LifeFacade.HandleDeath()`

### Cuando una Entidad Muere:

1. **LifeFacade.HandleDeath()** se ejecuta
2. **Reproduce sonido** de muerte (si está configurado)
3. **Spawnea efecto visual** de explosión (si está configurado)
4. **Suma puntos** al marcador (solo si `pointsOnDeath > 0`)
5. **Destruye/desactiva** el GameObject:
   - Jugador: `SetActive(false)`
   - Enemigo: `Destroy(gameObject)`

## ✅ Checklist de Configuración

### Jugador:
- [ ] LifeFacade existe en la escena
- [ ] LifeType_Player creado y configurado
- [ ] PlayerHealth tiene `playerLifeType` asignado
- [ ] PlayerPowerUpManager tiene `Shell_1` y `Shell_2` asignados
- [ ] BulletData del jugador tiene `damage > 0`

### Enemigos:
- [ ] LifeType_BasicEnemy creado y configurado
- [ ] EnemyLife tiene `enemyLifeType` asignado
- [ ] EnemyBulletData tiene `damage > 0`
- [ ] LifeType del enemigo tiene `pointsOnDeath > 0` (para otorgar puntos)

### Power-Ups de Escudo:
- [ ] PowerUp_Shield1 creado con `shieldResistance = 100`
- [ ] PowerUp_Shield2 creado con `shieldResistance = 200`
- [ ] Power-Ups tienen sus `PowerUpType` asignados

### LifeFacade:
- [ ] GameObject "LifeFacade" existe en la escena
- [ ] `defaultExplosionPrefab` asignado
- [ ] `scoreManager` asignado (GameObject con método `AddScore(int)`)

## 🎮 Ejemplo de Configuración Completa

### Jugador:
```
GameObject: Player
├── PlayerHealth
│   └── playerLifeType: LifeType_Player (maxHealth=100, hasShield=true)
├── EntityLife (automático)
├── PlayerPowerUpManager
│   ├── Shell_1: GameObject (con ShieldComponent automático)
│   └── Shell_2: GameObject (con ShieldComponent automático)
└── BulletShoot
    └── bulletData: PlayerBulletData (damage=10)
```

### Enemigo:
```
GameObject: Enemy
├── EnemyLife
│   └── enemyLifeType: LifeType_BasicEnemy (maxHealth=50, pointsOnDeath=100)
├── EntityLife (automático)
└── EnemyController
    └── enemyBulletData: EnemyBulletData (damage=5)
```

### Power-Up de Escudo:
```
GameObject: PowerUp_Shield
└── PowerUp
    └── powerUpType: PowerUp_Shield1 (shieldResistance=100)
```

## 🐛 Troubleshooting

**Problema**: Las balas no hacen daño
- ✅ Verifica que `BulletData.damage > 0`
- ✅ Verifica que el GameObject tenga `EntityLife` o `PlayerHealth`/`EnemyLife`
- ✅ Verifica que los tags estén correctos ("Player" y "Enemy")
- ✅ Verifica que los colliders sean triggers

**Problema**: Los enemigos no se destruyen al morir
- ✅ Verifica que `LifeFacade` exista en la escena
- ✅ Verifica que `LifeType` esté asignado en `EnemyLife`
- ✅ Verifica que `maxHealth` sea mayor que 0

**Problema**: El escudo no funciona
- ✅ Verifica que `PowerUpType.shieldResistance > 0`
- ✅ Verifica que `PlayerPowerUpManager` tenga `Shell_1` y `Shell_2` asignados
- ✅ Verifica que el `LifeType` del jugador tenga `hasShield = true`

**Problema**: No se suman puntos
- ✅ Verifica que `LifeType.pointsOnDeath > 0` (solo enemigos)
- ✅ Verifica que `LifeFacade.scoreManager` esté asignado
- ✅ Verifica que el scoreManager tenga un método `AddScore(int)`

## 📝 Notas Importantes

1. **El escudo se activa automáticamente** cuando se recoge un power-up de escudo
2. **El escudo absorbe daño** antes de que se reduzca la vida
3. **Shield 1 tiene resistencia 100**, Shield 2 tiene resistencia 200
4. **Las balas ya están configuradas** para hacer daño automáticamente
5. **La muerte se maneja centralmente** a través de `LifeFacade`

