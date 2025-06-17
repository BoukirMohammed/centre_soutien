// Dans Commands/RelayCommand.cs ou Helpers/RelayCommand.cs

using System;
using System.Windows.Input;

namespace centre_soutien.Commands // Ou centre_soutien.Helpers, adapte le namespace
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute; // Correction : _ au lieu de *
        private readonly Predicate<object?>? _canExecute; // Correction : _ au lieu de *

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute)); // Correction : _ au lieu de *
            _canExecute = canExecute; // Correction : _ au lieu de *
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object? parameter) => _execute(parameter); // Correction : _ au lieu de manquant

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}