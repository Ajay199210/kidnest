namespace KidNest.Core.Shared
{
    public class OperationResult
    {
        public bool Success => Errors.Count == 0;
        public List<string> Errors { get; } = [];
        public string? Message { get; set; }

        public void AddError(string error) => Errors.Add(error);

        public static OperationResult Ok() => new() { Message = "Operation completed successfully." };

        public static OperationResult Fail(string error)
        {
            var result = new OperationResult();
            result.AddError(error);
            
            return result;
        }

        public static OperationResult Fail(IEnumerable<string> errors)
        {
            var result = new OperationResult();
            result.Errors.AddRange(errors);
            
            return result;
        }
    }
}
