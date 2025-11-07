namespace OFMS_API.SerilogMiddleware
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ApiCodeAttribute : Attribute
    {
        public string Code { get; }
        public ApiCodeAttribute(string code)
        {
            Code = code;
        }
    }
}
