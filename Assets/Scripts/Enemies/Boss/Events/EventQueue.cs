using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventQueue : MonoBehaviour
{
    [SerializeField] private float delayBetweenCommands = 1f;
    private readonly Queue<ICommand> queue = new Queue<ICommand>();
    private bool isProcessing;

    public void AddEvent(ICommand command)
    {
        if (command == null) return;
        queue.Enqueue(command);
        ExecuteEvents();
    }

    public void ExecuteEvents()
    {
        if (!isProcessing) StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        isProcessing = true;
        while (queue.Count > 0)
        {
            var command = queue.Dequeue();
            command.Execute();
            yield return new WaitForSeconds(delayBetweenCommands);
        }
        isProcessing = false;
    }
}
