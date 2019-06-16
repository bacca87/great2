using NLog;
using System;

namespace Great.Utils
{
    public class BaseImport
    {
        protected Logger log = null;

        public delegate void OperationFinishedHandler(object source, bool failed);
        public delegate void StatusChangedHandler(object source, ImportArgs args);
        public delegate void MessageHandler(object source, ImportArgs args);
        public event OperationFinishedHandler OnFinish;
        public event StatusChangedHandler OnStatusChanged;
        public event MessageHandler OnMessage;

        protected BaseImport(Logger log) => this.log = log;

        public virtual void Start() { }
        public virtual void Cancel() { }
        public virtual void Close() { }

        protected void StatusChanged(string status)
        {
            OnStatusChanged?.Invoke(this, new ImportArgs(status));
            OnMessage?.Invoke(this, new ImportArgs(status));
            log.Info(status);
        }

        protected void Warning(string message)
        {
            OnMessage?.Invoke(this, new ImportArgs($"WARNING: {message}"));
            log.Warn(message);
        }

        protected void Error(string message, Exception ex = null)
        {
            OnMessage?.Invoke(this, new ImportArgs($"ERROR: {message}"));
            log.Error(ex, message);
        }

        protected void Message(string message)
        {
            OnMessage?.Invoke(this, new ImportArgs(message));
            log.Debug(message);
        }

        protected void Finished(bool isCompleted = true)
        {
            OnFinish?.Invoke(this, isCompleted);
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
