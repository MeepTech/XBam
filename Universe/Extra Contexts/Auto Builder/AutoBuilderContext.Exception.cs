namespace Meep.Tech.XBam.Configuration {

  public partial class AutoBuilderContext {
    /// <summary>
    /// An exception thrown by the auto-builder.
    /// </summary>
    public class Exception : System.ArgumentException {

      /// <summary>
      /// The model type that threw the exepction 
      /// </summary>
      public System.Type ModelTypeBeingBuilt { get; }

      ///<summary><inheritdoc/></summary>
      public Exception(System.Type modelType, string message, System.Exception innerException)
        : base(message, innerException) { ModelTypeBeingBuilt = modelType; }
    }
  }
}
