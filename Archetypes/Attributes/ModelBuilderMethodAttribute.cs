using System;

namespace Meep.Tech.XBam {

    /// <summary>
    /// Used to mark a function as a Model Builder Method.
    /// Public methods that produce an IModel and start with 'Make' are automatically considered Model Builder Methods. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ModelBuilderMethodAttribute : Attribute {}
}
