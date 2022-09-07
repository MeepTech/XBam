using Meep.Tech.XBam;
using Meep.Tech.XBam.Configuration;

namespace Meep.Tech.Data.Archetypes.Traits {

  /// <summary>
  /// Used to configure a model pre and post auto build.
  /// </summary>
  public interface IConfigureModelForAutoBuild : IArchetype , ITrait<IConfigureModelForAutoBuild> {
    string ITrait<IConfigureModelForAutoBuild>.TraitName
      => "Custom Model Auto Build Pre and Post Logic";
    string ITrait<IConfigureModelForAutoBuild>.TraitDescription
      => "Used for Archetypes that want to execute custom logic pre or post Auto Building";

    /// <summary>
    /// called before the auto build steps are run
    /// </summary>
    protected internal void OnAutoBuildInitialized(ref IModel model, IBuilder builder) { }

    /// <summary>
    /// called after the auto build steps are run
    /// </summary>
    protected internal void OnAutoBuildStepsCompleted(ref IModel model, IBuilder builder) { }
  }
}
