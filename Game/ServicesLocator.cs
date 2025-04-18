
//GESION DES SERVICES DU JEUX
public static class ServiceLocator
{
    private static Dictionary<Type, object> _services = new();

    //Enregistrement d'un service via un type avec constructeur de paramètre null
    public static void Register<T>() where T : new()
    {
        if (_services.ContainsKey(typeof(T)))
            throw new InvalidOperationException($"le type {typeof(T).Name}  est déjà enregistré dans le service locator");

        _services.Add(typeof(T), new T());
    }
    //Enregistrement d'un service via un objet
    public static void Register<T>(T pService)
    {
        if (_services.ContainsKey(typeof(T)))
            throw new InvalidOperationException($"le type {typeof(T).Name}  est déjà enregistré dans le service locator");

        _services.Add(typeof(T), pService);
    }

    //Désinscription d'un service du locator
    public static void UnRegister<T>()
    {
        if (_services.ContainsKey(typeof(T)))
        {
            if(typeof(T).IsAssignableTo(typeof(IDisposable))) ((IDisposable)_services[typeof(T)]).Dispose();
            _services.Remove(typeof(T));
        }
    }

    //Récupération d'un service du locator
    public static T Get<T>()
    {
        if (!_services.ContainsKey(typeof(T)))
            throw new InvalidOperationException($"Le type {typeof(T).Name} doit être enregistré dans le service locator");

        return (T)_services[typeof(T)];
    }

}