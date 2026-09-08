using HealthDataCollector.Enums;

namespace HealthDataCollector.Data.Control.Entities

{

    public class Setting
    {
        public int Id { get; set; }

        public string Key { get; set; } = string.Empty;

        public SettingDataType DataType { get; set; }
        
        public string? MaxValue {  get; set; } = string.Empty;
        
        public string? MinValue { get; set; } = string.Empty;
        
        public string? Value { get; set; }
    }
}
