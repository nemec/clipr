namespace clipr.Validation
{
    public interface IParseValidator<T>
    {
         ValidationResult Validate(T component);
    }
}