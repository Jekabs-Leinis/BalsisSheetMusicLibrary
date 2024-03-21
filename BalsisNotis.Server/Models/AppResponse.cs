namespace BalsisNotis.Server.Models
{
    public class AppResponse<T>(T? model, bool success = false, string error = "") where T : class
    {
        public T? Model { get; set; } = model;

        public bool Success { get; set; } = success;

        public string Error { get; set; } = error;
    }
}
