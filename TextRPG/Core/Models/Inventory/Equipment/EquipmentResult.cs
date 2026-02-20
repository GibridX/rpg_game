using System;
using TextRPG.Core.Models;

namespace TextRPG.Core.Models.Inventory.Equipment
{
    public class EquipmentResult
    {
        public bool Success { get; }
        public string Message { get; } 

        public EquipmentResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}