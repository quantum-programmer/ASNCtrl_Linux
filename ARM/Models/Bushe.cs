using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ARM.Models
{
    public partial class Bushe : ObservableObject
    {
        [ObservableProperty]
        private bool _isHiddenButtonVisible = true;

        [ObservableProperty]
        private bool _isInputPairVisible = false;

        [ObservableProperty]
        private string _text = string.Empty;

        [ObservableProperty]
        private string _buttonContent = "Настроить";

        [ObservableProperty]
        private string _placeholderText = "Введите значение";

        [ObservableProperty]
        private string _unit = ""; // Единица измерения (л, кг, etc.)

        [ObservableProperty]
        private bool _isValid = true;

        [ObservableProperty]
        private string _validationError = "";

        // Метод для валидации введенных данных
        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                IsValid = false;
                ValidationError = "Значение не может быть пустым";
                return false;
            }

            // Дополнительная логика валидации в зависимости от типа контрола
            IsValid = true;
            ValidationError = "";
            return true;
        }
    }
}
