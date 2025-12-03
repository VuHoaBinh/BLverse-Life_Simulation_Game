import sys
import onnx
from onnx import numpy_helper
import numpy as np
import matplotlib.pyplot as plt


def list_initializers(model):
    """Return list of (name, shape) for initializers in model."""
    out = []
    for init in model.graph.initializer:
        arr = numpy_helper.to_array(init)
        out.append((init.name, arr.shape))
    return out


def get_actor_weights(onnx_path, verbose=True):
    """Load ONNX and try to collect actor-related weights.

    Heuristics:
    - First look for initializer names containing 'actor' or 'policy'
    - Next, fall back to any initializer name containing 'weight', 'bias', 'fc', 'linear', 'action', 'pi'
    - If still nothing, raise ValueError but include available initializers in message
    """
    model = onnx.load(onnx_path)

    candidates = []
    for initializer in model.graph.initializer:
        name = initializer.name.lower()
        candidates.append((name, initializer))

    found = []

    # Primary heuristics
    for name, initializer in candidates:
        if any(tok in name for tok in ("actor", "policy", "pi")):
            arr = numpy_helper.to_array(initializer)
            found.append(arr.flatten())

    # Secondary heuristics
    if not found:
        for name, initializer in candidates:
            if any(tok in name for tok in ("weight", "bias", "fc", "linear", "action")):
                arr = numpy_helper.to_array(initializer)
                found.append(arr.flatten())

    if not found:
        # build informative message
        init_list = list_initializers(model)
        msg_lines = ["No Actor weights found in ONNX file using heuristics."]
        msg_lines.append("Available initializers (name, shape):")
        for n, s in init_list:
            msg_lines.append(f"  - {n} : {s}")
        raise ValueError("\n".join(msg_lines))

    return np.concatenate(found)


def plot_weights_hist(weights, bins=80, title="Histogram of Actor Weights Distribution"):
    plt.figure(figsize=(10, 5))
    plt.hist(weights, bins=bins, color='red')
    plt.title(title)
    plt.xlabel("Weight Value")
    plt.ylabel("Frequency")
    plt.grid(True)
    plt.tight_layout()
    plt.show()


if __name__ == '__main__':
    # allow passing ONNX path as CLI arg
    if len(sys.argv) > 1:
        onnx_file = sys.argv[1]
    else:
        onnx_file = r"D:\BLverse-Life_Simulation_Game\Assets\Episode\RLnotBC_01\RLnotBC_01\GridBrain.onnx"

    try:
        weights = get_actor_weights(onnx_file)
    except Exception as e:
        print("Error while extracting weights:")
        print(e)
        sys.exit(1)

    plot_weights_hist(weights)
