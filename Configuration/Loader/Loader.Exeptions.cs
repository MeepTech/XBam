using Meep.Tech.Reflection;
using System;

namespace Meep.Tech.XBam.Configuration {
  public partial class Loader {

    /// <summary>
    /// Exeption thrown when you fail to initialize or finalize a type. This will cause the loader to retry for things like missing dependencies that haven't loaded yet:
    /// </summary>
    public class FailedToConfigureResourceException : InvalidOperationException {
      public FailedToConfigureResourceException(string message) : base(message) { }
      public FailedToConfigureResourceException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exeption thrown when you fail to initialize or finalize an archetype. This will cause the loader to retry for things like missing dependencies that haven't loaded yet:
    /// </summary>
    public class FailedToConfigureNewArchetypeException : FailedToConfigureResourceException {
      public FailedToConfigureNewArchetypeException(string message) : base(message) { }
      public FailedToConfigureNewArchetypeException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exeption thrown when you fail to initialize or finalize a model. This will cause the loader to retry for things like missing dependencies that haven't loaded yet:
    /// </summary>
    public class FailedToConfigureNewModelException : FailedToConfigureResourceException {
      public FailedToConfigureNewModelException(string message) : base(message) { }
      public FailedToConfigureNewModelException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exeption thrown when you fail to initialize or finalize a component. This will cause the loader to retry for things like missing dependencies that haven't loaded yet:
    /// </summary>
    public class FailedToConfigureNewComponentException : FailedToConfigureResourceException {
      public FailedToConfigureNewComponentException(string message) : base(message) { }
      public FailedToConfigureNewComponentException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exeption thrown when you fail to initialize an archetype because another archetype is missing. This will cause the loader to retry for things like missing dependencies that haven't loaded yet:
    /// </summary>
    public class MissingArchetypeDependencyException : FailedToConfigureNewArchetypeException {
      public MissingArchetypeDependencyException(string message) : base(message) { }
      public MissingArchetypeDependencyException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exeption thrown when you cannot to initialize a type. This will cause the loader to stop trying for this archetype and mark it as failed completely:
    /// </summary>
    public class CannotInitializeResourceException : InvalidOperationException {
      public CannotInitializeResourceException(string message) : base(message) { }
      public CannotInitializeResourceException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exeption thrown when you cannot to initialize an archetype. This will cause the loader to stop trying for this archetype and mark it as failed completely.
    /// </summary>
    public class CannotInitializeArchetypeException : CannotInitializeResourceException {
      public CannotInitializeArchetypeException(string message) : base(message) { }
      public CannotInitializeArchetypeException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exeption thrown when you cannot to initialize a model type.  This will cause the loader to stop trying for this model and mark it as failed completely.
    /// </summary>
    public class CannotInitializeModelException : CannotInitializeResourceException {
      public CannotInitializeModelException(string message) : base(message) { }
      public CannotInitializeModelException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exeption thrown when you cannot to initialize a component type.  This will cause the loader to stop trying for this component and mark it as failed completely.
    /// </summary>
    public class CannotInitializeComponentException : CannotInitializeResourceException {
      public CannotInitializeComponentException(string message) : base(message) { }
      public CannotInitializeComponentException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exeption thrown when a component is missing
    /// </summary>
    public class MissingComponentDependencyForArchetypeException : MissingDependencyException {
      public MissingComponentDependencyForArchetypeException(string message) : base(message) { }
      public MissingComponentDependencyForArchetypeException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exeption thrown when a dependency for an archetype is missing
    /// </summary>
    public class MissingDependencyForArchetypeException : MissingDependencyException {
      public MissingDependencyForArchetypeException(string message) : base(message) { }
      public MissingDependencyForArchetypeException(string message, Exception innerException) : base(message, innerException) { }
      public MissingDependencyForArchetypeException(System.Type dependentType, System.Type missingDependency)
        : base("Archetype", dependentType, missingDependency) { }
    }

    /// <summary>
    /// Exeption thrown when a dependency for a model is missing
    /// </summary>
    public class MissingDependencyForModelException : MissingDependencyException {
      public MissingDependencyForModelException(string message) : base(message) { }
      public MissingDependencyForModelException(string message, Exception innerException) : base(message, innerException) { }
      public MissingDependencyForModelException(System.Type dependentType, System.Type missingDependency)
        : base("Model", dependentType, missingDependency) { }
    }

    /// <summary>
    /// Exeption thrown when a dependency for a component is missing
    /// </summary>
    public class MissingDependencyForComponentException : MissingDependencyException {
      public MissingDependencyForComponentException(string message) : base(message) { }
      public MissingDependencyForComponentException(string message, Exception innerException) : base(message, innerException) { }
      public MissingDependencyForComponentException(System.Type dependentType, System.Type missingDependency)
        : base("Component", dependentType, missingDependency) { }
    }

    /// <summary>
    /// Exeption thrown when a dependency for a type is missing
    /// </summary>
    public class MissingDependencyException : FailedToConfigureResourceException {
      public MissingDependencyException(string message) : base(message) { }
      public MissingDependencyException(string message, Exception innerException) : base(message, innerException) { }
      internal MissingDependencyException(string type, Type dependentType, System.Type missingDependency) : base(
        $"Missing dependency: {missingDependency.ToFullHumanReadableNameString()} for {type}: {dependentType.ToFullHumanReadableNameString()}"
      ) { }
    }
  }
}