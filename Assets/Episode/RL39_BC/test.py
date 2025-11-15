# ...existing code...
import argparse
import onnx
import numpy as np
import matplotlib.pyplot as plt
from onnx import numpy_helper

def list_initializers(model):
    for init in model.graph.initializer:
        arr = numpy_helper.to_array(init)
        print(f"{init.name}: shape={arr.shape}, dtype={arr.dtype}")

def load_initializers(model):
    return {init.name: numpy_helper.to_array(init) for init in model.graph.initializer}

def plot_hist(all_weights, save=None):
    vals = np.concatenate([w.ravel() for w in all_weights if w.size>0])
    plt.figure(figsize=(6,4))
    plt.hist(vals, bins=100, color='C0', alpha=0.7)
    plt.title('Weight distribution'); plt.xlabel('Value'); plt.ylabel('Count'); plt.grid(True)
    if save: plt.savefig(save, dpi=150)
    plt.show()

def plot_kernel(kernel, cmap='viridis', save=None):
    # kernel: e.g. conv kernel shape (out_ch, in_ch, kh, kw) or (kh, kw) etc.
    if kernel.ndim == 4:
        k = kernel[0,0]  # show first out,first in
    elif kernel.ndim == 3:
        k = kernel[0]
    elif kernel.ndim == 2:
        k = kernel
    else:
        raise ValueError("Unsupported kernel ndim")
    plt.figure(figsize=(4,4))
    plt.imshow(k, cmap=cmap, aspect='equal')
    plt.colorbar()
    plt.title('Kernel visualization')
    if save: plt.savefig(save, dpi=150)
    plt.show()

# new: line & scatter plot helpers
def plot_line(arr, save=None):
    y = arr.ravel()
    x = np.arange(y.size)
    plt.figure(figsize=(8,4))
    plt.plot(x, y, '-', color='C1', linewidth=0.8)
    plt.xlabel('Index'); plt.ylabel('Value'); plt.title('Line plot of weights'); plt.grid(True)
    if save: plt.savefig(save, dpi=150)
    plt.show()

def plot_scatter(arr, save=None):
    y = arr.ravel()
    x = np.arange(y.size)
    plt.figure(figsize=(8,4))
    plt.scatter(x, y, s=6, color='C2', alpha=0.6)
    plt.xlabel('Index'); plt.ylabel('Value'); plt.title('Scatter (decay) plot of weights'); plt.grid(True)
    if save: plt.savefig(save, dpi=150)
    plt.show()

def main():
    p = argparse.ArgumentParser()
    p.add_argument('onnx', help='path to .onnx')
    p.add_argument('--list', action='store_true', help='list initializers')
    p.add_argument('--hist', action='store_true', help='plot histogram of all weights')
    p.add_argument('--plot', metavar='NAME', help='plot kernel by initializer name')
    p.add_argument('--save', metavar='FILE', help='save plot to file')
    p.add_argument('--line', action='store_true', help='plot line chart of weights (flattened)')
    p.add_argument('--scatter', action='store_true', help='plot scatter chart of weights (flattened)')
    args = p.parse_args()

    model = onnx.load(args.onnx)
    if args.list:
        list_initializers(model)
        return

    inits = load_initializers(model)

    # choose source array: named initializer if --plot used, otherwise concat all
    source_arr = None
    if args.plot:
        name = args.plot
        if name not in inits:
            print("Initializer not found. Use --list to see names.")
            return
        source_arr = inits[name]

    if args.hist:
        # histogram uses all weights by default
        plot_hist(list(inits.values()), save=args.save)
        return

    if args.line or args.scatter:
        if source_arr is None:
            # flatten all initializers into one vector
            source_arr = np.concatenate([w.ravel() for w in inits.values()]) if inits else np.array([])
            if source_arr.size == 0:
                print("No weights found to plot.")
                return
        if args.line:
            plot_line(source_arr, save=args.save)
        if args.scatter:
            plot_scatter(source_arr, save=args.save)
        return

    # fallback: if user asked --plot (kernel image) show kernel visualization
    if args.plot:
        plot_kernel(inits[args.plot], save=args.save)
        return

    print("No plot option selected. Use --list, --hist, --line, --scatter or --plot NAME.")
    # ...existing code...
if __name__ == "__main__":
    main()
# ...existing code...