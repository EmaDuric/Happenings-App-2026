namespace Happenings.Model.Exceptions;

// Vlastiti exception tipovi za poslovne slucajeve. ExceptionMiddleware ih mapira
// na HTTP status po TIPU (ne po tekstu poruke).

// 404 � trazeni resurs ne postoji
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

// 400 � krsenje poslovnog pravila / nevalidan zahtjev
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}

// 403 � korisnik nema pravo na ovu akciju/resurs
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}

// 409 � konflikt sa trenutnim stanjem (duplikat, vec obradjeno, itd.)
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

// 401 � neuspjesna autentikacija
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}
