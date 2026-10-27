using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Estructura de datos especializada para gestionar los 10 mejores puntajes.
/// Mantiene los elementos estrictamente ordenados de mayor a menor y acotados a la capacidad máxima.
/// </summary>
[Serializable]
public class TopScoresCollection : IEnumerable<HighScoreManager.Entry>
{
    private const int DefaultCapacity = 10;

    [SerializeField] private int capacity;
    [SerializeField] private List<HighScoreManager.Entry> items;

    public int Capacity => capacity;
    public int Count => items != null ? items.Count : 0;

    public HighScoreManager.Entry this[int index]
    {
        get
        {
            if (items == null || index < 0 || index >= items.Count)
                throw new IndexOutOfRangeException("Índice de puntaje fuera de rango: " + index);
            return items[index];
        }
    }

    public TopScoresCollection(int capacity = DefaultCapacity)
    {
        this.capacity = capacity > 0 ? capacity : DefaultCapacity;
        this.items = new List<HighScoreManager.Entry>(this.capacity);
    }

    public TopScoresCollection(IEnumerable<HighScoreManager.Entry> initialEntries, int capacity = DefaultCapacity) : this(capacity)
    {
        if (initialEntries != null)
        {
            foreach (var entry in initialEntries)
            {
                Add(entry);
            }
        }
    }

    /// <summary>
    /// Inserta un puntaje en la posición ordenada correspondiente (de mayor a menor).
    /// Si la lista supera los 10 elementos, descarta automáticamente el menor puntaje.
    /// Retorna el rango obtenido (1 a 10), o -1 si no clasificó.
    /// </summary>
    public int Add(HighScoreManager.Entry newEntry)
    {
        if (newEntry == null) return -1;
        if (items == null) items = new List<HighScoreManager.Entry>(capacity);

        // Búsqueda lineal/binaria para inserción ordenada descendente
        int insertIndex = 0;
        while (insertIndex < items.Count && items[insertIndex].score >= newEntry.score)
        {
            insertIndex++;
        }

        // Si ya está en capacidad máxima y el puntaje no supera a ninguno en el top
        if (insertIndex >= capacity)
        {
            return -1;
        }

        items.Insert(insertIndex, newEntry);

        // Garantizar que nunca supere los 10 mejores
        while (items.Count > capacity)
        {
            items.RemoveAt(items.Count - 1);
        }

        return insertIndex + 1; // Rango 1-based
    }

    public int Add(string initials, int score)
    {
        return Add(new HighScoreManager.Entry { initials = initials, score = score });
    }

    public bool IsTopScore(int score)
    {
        if (items == null || items.Count < capacity) return true;
        return score > items[items.Count - 1].score;
    }

    public void Clear()
    {
        if (items != null) items.Clear();
    }

    public List<HighScoreManager.Entry> ToList()
    {
        return items != null ? new List<HighScoreManager.Entry>(items) : new List<HighScoreManager.Entry>();
    }

    public HighScoreManager.Entry[] ToArray()
    {
        return items != null ? items.ToArray() : new HighScoreManager.Entry[0];
    }

    public IEnumerator<HighScoreManager.Entry> GetEnumerator()
    {
        if (items == null) items = new List<HighScoreManager.Entry>(capacity);
        return items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
