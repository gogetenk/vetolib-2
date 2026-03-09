using Microsoft.ML.Data;

namespace Vetolib.AI.Application.ML;

/// <summary>
/// ML.NET output schema for the no-show binary classification model.
/// </summary>
internal sealed class NoShowOutput
{
    /// <summary>Predicted label: true = likely no-show.</summary>
    [ColumnName("PredictedLabel")]
    public bool PredictedLabel { get; set; }

    /// <summary>Probability of a no-show (0.0 to 1.0).</summary>
    [ColumnName("Probability")]
    public float Probability { get; set; }

    /// <summary>Raw model score before sigmoid transformation.</summary>
    [ColumnName("Score")]
    public float Score { get; set; }
}
