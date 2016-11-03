using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HighlightedTextBlock
{
    public class TaskQueue
    {
        Task _worker;
        Queue<Action> _queue;
        int _maxTasks;
        bool _deleteOld;
        object _lock = new object();

        public TaskQueue(int maxTasks, bool deleteOld = true)
        {
            if (maxTasks < 1)
                throw new ArgumentException("TaskQueue: максимальное число задач должно быть больше 0");
            _maxTasks = maxTasks;
            _deleteOld = deleteOld;
            _queue = new Queue<Action>(maxTasks);
        }

        public bool Add(Action action)
        {
            if (_queue.Count() < _maxTasks)
            {
                _queue.Enqueue(action);
                DoWorkAsync();
                return true;
            }
            if (_deleteOld)
            {
                _queue.Dequeue();
                return Add(action);
            }
            return false;
        }

        void DoWorkAsync()
        {
            if (_queue.Count > 0)
                _worker = Task.Factory.StartNew(DoWork);
        }

        void DoWork()
        {
            lock (_lock)
            {
                if (_queue.Count > 0)
                {
                    _queue.Dequeue().Invoke();
                    DoWork();
                }
            }
        }
    }
}
