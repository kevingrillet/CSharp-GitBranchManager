using System;
using System.Diagnostics;
using System.Windows.Input;

namespace CSharp_GitBranchManager.Utils
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Func<T, bool> _canExecute;
        private readonly Action<T> _execute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<T> execute) : this(execute, null)
        {
        }

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            if (parameter == null)
            {
                return _canExecute == null || _canExecute(default);
            }
            if (parameter is T typedParameter)
            {
                return _canExecute == null || _canExecute(typedParameter);
            }
            return false;
        }

        public void Execute(object parameter)
        {
            if (parameter is T typedParameter)
            {
                _execute(typedParameter);
            }
            else
            {
                throw new ArgumentException($"The parameter must be of type {typeof(T).Name}", nameof(parameter));
            }
        }
    }
}
