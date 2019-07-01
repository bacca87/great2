using NLog;
using System;
using System.Collections.Concurrent;

namespace Great.Utils
{
    public class BaseImport
    {
        #region Properties
        protected Logger log = null;

        public ConcurrentQueue<string> Output { get; internal set; }

        private string _status;
        public string Status
        {
            get
            {
                lock(this)
                {
                    return _status;
                }
            }
            internal set
            {
                lock(this)
                {
                    _status = value;
                }
            }
        }

        private bool _IsCompleted;
        public bool IsCompleted
        {
            get
            {
                lock (this)
                {
                    return _IsCompleted;
                }
            }
            internal set
            {
                lock (this)
                {
                    _IsCompleted = value;
                }
            }
        }

        private bool _IsCancelled;
        public bool IsCancelled
        {
            get
            {
                lock (this)
                {
                    return _IsCancelled;
                }
            }
            internal set
            {
                lock (this)
                {
                    _IsCancelled = value;
                }
            }
        }
        #endregion

        protected BaseImport(Logger log)
        {
            IsCompleted = false;
            this.log = log;
            Output = new ConcurrentQueue<string>();
        }

        public virtual void Start() { }
        public virtual void Cancel() { }
        public virtual void Close() { }

        protected void StatusChanged(string status)
        {
            Status = status;
            Message(status);
            log.Info(status);
        }

        protected void Warning(string message)
        {
            Message($"WARNING: {message}");
            log.Warn(message);
        }

        protected void Error(string message, Exception ex = null)
        {
            Message($"ERROR: {message}");
            log.Error(ex, message);
        }

        protected void Message(string message)
        {
            Output.Enqueue(message);
            log.Debug(message);
        }

        protected void Finished(bool isCompleted = true)
        {
            IsCompleted = isCompleted;
            IsCancelled = !isCompleted;
        }
    }

    public class ImportArgs : EventArgs
    {
        public ImportArgs(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}
