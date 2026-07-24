namespace ExpenSR.Exceptions
{
    // Maps to 404
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    // Maps to 409
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }

    // Maps to 401
    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException(string message = "Invalid email or password.")
            : base(message) { }
    }

    // Maps to 403 - account exists but isn't allowed to log in yet
    public class AccountNotApprovedException : Exception
    {
        public AccountNotApprovedException(string message) : base(message) { }
    }

    // Maps to 400
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
}