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
    vals = np.concatenate([w.ravel() for w in all_weights if w.size > 0]) if all_weights else np.array([])
    if vals.size == 0:
        print("No weights to plot histogram.")
        return
    plt.figure(figsize=(6, 4))
    plt.hist(vals, bins=100, color='C0', alpha=0.7)
    # add mean and median lines
    mean_val = float(np.mean(vals))
    median_val = float(np.median(vals))
    plt.axvline(mean_val, color='k', linestyle='--', linewidth=1, label=f'mean={mean_val:.4g}')
    plt.axvline(median_val, color='orange', linestyle=':', linewidth=1, label=f'median={median_val:.4g}')
    plt.title('Weight distribution')
    plt.xlabel('Value')
    plt.ylabel('Count')
    plt.grid(True)
    plt.legend()
    if save:
        plt.savefig(save, dpi=150)
    plt.show()


def plot_kernel(kernel, cmap='viridis', save=None):
    if kernel.ndim == 4:
        k = kernel[0, 0]
    elif kernel.ndim == 3:
        k = kernel[0]
    elif kernel.ndim == 2:
        k = kernel
    else:
        raise ValueError("Unsupported kernel ndim")

    plt.figure(figsize=(4, 4))
    plt.imshow(k, cmap=cmap, aspect='equal')
    plt.colorbar()
    plt.title('Kernel visualization')
    if save:
        plt.savefig(save, dpi=150)
    plt.show()


def plot_line(arr, save=None):
    y = arr.ravel()
    if y.size == 0:
        print("No data to plot line.")
        return
    x = np.arange(y.size)
    plt.figure(figsize=(8, 4))
    plt.plot(x, y, '-', color='C1', linewidth=0.8)
    plt.xlabel('Index')
    plt.ylabel('Value')
    plt.title('Line plot of weights')
    plt.grid(True)
    if save:
        plt.savefig(save, dpi=150)
    plt.show()


def plot_scatter(arr, save=None):
    y = arr.ravel()
    if y.size == 0:
        print("No data to plot scatter.")
        return
    x = np.arange(y.size)
    plt.figure(figsize=(8, 4))
    plt.scatter(x, y, s=6, color='C2', alpha=0.6)
    plt.xlabel('Index')
    plt.ylabel('Value')
    plt.title('Scatter plot')
    plt.grid(True)
    if save:
        plt.savefig(save, dpi=150)
    plt.show()


# ----------------------------- COMPARE FUNCTIONS -----------------------------

def compare_hist(init_name, inits1, inits2, save=None):
    if init_name:
        if init_name not in inits1 or init_name not in inits2:
            print("Initializer not found in one of the models. Use --list on each model to check names.")
            return
        vals1 = inits1[init_name].ravel()
        vals2 = inits2[init_name].ravel()
    else:
        vals1 = np.concatenate([w.ravel() for w in inits1.values()]) if inits1 else np.array([])
        vals2 = np.concatenate([w.ravel() for w in inits2.values()]) if inits2 else np.array([])

    if vals1.size == 0 and vals2.size == 0:
        print("No weights to compare.")
        return

    plt.figure(figsize=(8, 5))
    plt.hist(vals1, bins=100, alpha=0.7, label='PPO', color='red')
    plt.hist(vals2, bins=100, alpha=0.7, label='PPO+BC', color='blue')
    # add mean/median lines for each distribution when possible
    try:
        if vals1.size:
            m1 = float(np.mean(vals1))
            med1 = float(np.median(vals1))
            plt.axvline(m1, color='darkred', linestyle='--', linewidth=1, label=f'PPO mean={m1:.4g}')
            plt.axvline(med1, color='orangered', linestyle=':', linewidth=1, label=f'PPO median={med1:.4g}')
    except Exception:
        pass
    try:
        if vals2.size:
            m2 = float(np.mean(vals2))
            med2 = float(np.median(vals2))
            plt.axvline(m2, color='darkblue', linestyle='--', linewidth=1, label=f'PPO+BC mean={m2:.4g}')
            plt.axvline(med2, color='deepskyblue', linestyle=':', linewidth=1, label=f'PPO+BC median={med2:.4g}')
    except Exception:
        pass
    plt.title(f'Weight Distribution Comparison ({init_name or "All"})')
    plt.xlabel('Value')
    plt.ylabel('Count')
    plt.legend()
    plt.grid(True)
    if save:
        plt.savefig(save, dpi=150)
    plt.show()


def compute_metrics(inits, name='Model'):
    all_w = [w for w in inits.values() if w.size > 0]
    if not all_w:
        print(f"{name} Metrics: no parameters found.")
        return {'norm': 0.0, 'var': 0.0}
    norms = [np.linalg.norm(w) for w in all_w]
    vars_ = [np.var(w) for w in all_w]

    print(f"{name} Metrics:")
    print(f"  Avg L2-Norm: {np.mean(norms):.4f}")
    print(f"  Avg Variance: {np.mean(vars_):.4f}")
    print(f"  Total Params: {sum(w.size for w in all_w)}")

    return {'norm': float(np.mean(norms)), 'var': float(np.mean(vars_))}


# ----------------------------- MAIN PROGRAM ----------------------------------


def main():
    p = argparse.ArgumentParser()
    p.add_argument('onnx', nargs='?', help='path to .onnx (required unless --compare)')
    p.add_argument('--list', action='store_true', help='list initializers')
    p.add_argument('--hist', action='store_true', help='plot histogram')
    p.add_argument('--plot', metavar='NAME', help='plot kernel by initializer name')
    p.add_argument('--colhist', action='store_true', help='column-wise histogram for 2D kernels')
    p.add_argument('--box', action='store_true', help='boxplot across columns for 2D kernels')
    p.add_argument('--heatmap', action='store_true', help='heatmap for 2D kernel')
    p.add_argument('--show-fliers', action='store_true', help='show outliers in boxplot')
    p.add_argument('--cols', metavar='LIST', help='comma-separated column indices to plot (e.g. 0,3,5)')
    p.add_argument('--max-cols', type=int, default=8, help='max columns for column-wise hist')
    p.add_argument('--save', metavar='FILE', help='save plot')


    def plot_column_histograms(kernel, cols=None, max_cols=8, save=None):
        if kernel.ndim != 2:
            raise ValueError("Column histograms require a 2D kernel")
        nrows, ncols = kernel.shape
        if ncols == 0:
            print("Empty kernel: no columns to plot.")
            return
        if cols:
            cols_to_plot = [c for c in cols if 0 <= c < ncols]
            if not cols_to_plot:
                print("No valid columns selected.")
                return
        else:
            cols_to_plot = list(range(min(ncols, max_cols)))
            if ncols > max_cols:
                print(f"Kernel has {ncols} columns; plotting first {max_cols} columns.")

        n = len(cols_to_plot)
        cols = 2
        rows = (n + cols - 1) // cols
        fig, axes = plt.subplots(rows, cols, figsize=(cols * 4, rows * 3))
        axes = axes.flatten() if hasattr(axes, 'flatten') else [axes]
        for i, c in enumerate(cols_to_plot):
            ax = axes[i]
            col_vals = kernel[:, c].ravel()
            ax.hist(col_vals, bins=60, color='C3', alpha=0.7)
            # draw mean line and annotate
            try:
                col_mean = float(np.mean(col_vals))
                ax.axvline(col_mean, color='k', linestyle='--', linewidth=0.9)
                ax.text(0.97, 0.85, f"μ={col_mean:.4g}", transform=ax.transAxes,
                        horizontalalignment='right', fontsize=8, bbox=dict(facecolor='white', alpha=0.6, edgecolor='none'))
            except Exception:
                pass
            ax.set_title(f'Column {c}')
            ax.set_xlabel('Value')
            ax.set_ylabel('Count')
            ax.grid(True)

        # hide any extra axes
        for j in range(i + 1, len(axes)):
            fig.delaxes(axes[j])

        fig.suptitle('Column-wise Histograms')
        fig.tight_layout(rect=[0, 0, 1, 0.96])
        if save:
            plt.savefig(save, dpi=150)
        plt.show()


    def plot_boxplot(kernel, cols=None, showfliers=False, save=None):
        if kernel.ndim != 2:
            raise ValueError("Boxplot requires a 2D kernel")
        nrows, ncols = kernel.shape
        if cols:
            cols = [c for c in cols if 0 <= c < ncols]
            if not cols:
                print("No valid columns selected for boxplot.")
                return
            data = [kernel[:, c].ravel() for c in cols]
            labels = [str(i) for i in cols]
        else:
            data = [kernel[:, c].ravel() for c in range(ncols)]
            labels = [str(i) for i in range(ncols)]

        n = len(data)
        width = max(6, n * 0.25)
        plt.figure(figsize=(width, 5))
        bxp = plt.boxplot(data, labels=labels, showfliers=showfliers, patch_artist=False)
        # show mean as a diamond marker
        means = [np.mean(d) if len(d) else 0 for d in data]
        x = np.arange(1, n + 1)
        # plt.plot(x, means, 'D', color='orange', markersize=4, label='Mean')
        plt.xlabel('Column')
        plt.ylabel('Value')
        plt.title('Boxplot per Column')
        plt.grid(True, axis='y')
        # adjust x labels for readability
        if n > 20:
            plt.xticks(rotation=45, fontsize=6)
        elif n > 10:
            plt.xticks(rotation=30, fontsize=8)

        if showfliers:
            plt.legend()

        if save:
            plt.savefig(save, dpi=150, bbox_inches='tight')
        plt.show()


    def plot_heatmap(kernel, cmap='viridis', save=None):
        if kernel.ndim != 2:
            raise ValueError("Heatmap requires a 2D kernel")
        plt.figure(figsize=(8, 6))
        plt.imshow(kernel, cmap=cmap, aspect='auto')
        plt.colorbar()
        plt.xlabel('Column')
        plt.ylabel('Row')
        plt.title('Kernel Heatmap')
        if save:
            plt.savefig(save, dpi=150)
        plt.show()
    p.add_argument('--line', action='store_true', help='line plot')
    p.add_argument('--scatter', action='store_true', help='scatter plot')

    # compare mode
    p.add_argument('--compare', nargs=2, metavar=('ONNX1', 'ONNX2'),
                   help='Compare two ONNX models: PPO vs PPO+BC')
    p.add_argument('--layer', metavar='NAME', help='Initializer for compare (optional)')

    args = p.parse_args()
    # parse cols list if provided
    args.cols_list = None
    if args.cols:
        try:
            args.cols_list = [int(x) for x in args.cols.split(',') if x.strip() != '']
        except Exception:
            print("Invalid --cols format. Use comma-separated integers like 0,3,5")
            return

    # --------------------- COMPARE MODE ---------------------
    if args.compare:
        try:
            model1 = onnx.load(args.compare[0])
            model2 = onnx.load(args.compare[1])
        except Exception as e:
            print("Failed to load one of the ONNX files:", e)
            return

        inits1 = load_initializers(model1)
        inits2 = load_initializers(model2)

        m1 = compute_metrics(inits1, 'PPO')
        m2 = compute_metrics(inits2, 'PPO+BC')

        if args.hist:
            compare_hist(args.layer, inits1, inits2, save=args.save)

        if args.line:
            arr1 = np.concatenate([w.ravel() for w in inits1.values()]) if inits1 else np.array([])
            arr2 = np.concatenate([w.ravel() for w in inits2.values()]) if inits2 else np.array([])
            x1 = np.arange(arr1.size)
            x2 = np.arange(arr2.size)
            plt.figure(figsize=(10, 5))
            if arr1.size:
                plt.plot(x1, arr1, 'r-', label='PPO', alpha=0.6)
            if arr2.size:
                plt.plot(x2, arr2, 'b-', label='PPO+BC', alpha=0.6)
            plt.title('Line Compare')
            plt.legend()
            plt.grid(True)
            if args.save:
                plt.savefig(args.save, dpi=150)
            plt.show()

        if args.scatter:
            arr1 = np.concatenate([w.ravel() for w in inits1.values()]) if inits1 else np.array([])
            arr2 = np.concatenate([w.ravel() for w in inits2.values()]) if inits2 else np.array([])
            x1 = np.arange(arr1.size)
            x2 = np.arange(arr2.size)
            plt.figure(figsize=(10, 5))
            if arr1.size:
                plt.scatter(x1, arr1, s=3, color='red', alpha=0.5, label='PPO')
            if arr2.size:
                plt.scatter(x2, arr2, s=3, color='blue', alpha=0.5, label='PPO+BC')
            plt.title('Scatter Compare')
            plt.legend()
            plt.grid(True)
            if args.save:
                plt.savefig(args.save, dpi=150)
            plt.show()

        print("\nDelta (PPO+BC - PPO):")
        print(f"  Norm Δ = {m2['norm'] - m1['norm']:.4f}")
        print(f"  Var  Δ = {m2['var'] - m1['var']:.4f}")
        return

    # --------------------- SINGLE MODEL MODE ---------------------
    if not args.onnx:
        print("Error: no ONNX provided. Use: script.py model.onnx or --compare a b")
        return

    try:
        model = onnx.load(args.onnx)
    except Exception as e:
        print("Failed to load ONNX file:", e)
        return

    if args.list:
        list_initializers(model)
        return

    inits = load_initializers(model)

    if not inits:
        print("No initializers found in model.")
        return

    # --- New: allow heatmap/box/colhist without --plot: apply to all 2D initializers ---
    def _apply_to_all_2d(func):
        names_2d = [n for n, k in inits.items() if getattr(k, 'ndim', 0) == 2]
        if not names_2d:
            print("No 2D initializers found for this model.")
            return
        for i, name in enumerate(names_2d):
            kernel = inits[name]
            # build save filename if requested and multiple outputs
            save = None
            if args.save:
                if len(names_2d) == 1:
                    save = args.save
                else:
                    base = args.save
                    if '.' in base:
                        pref, ext = base.rsplit('.', 1)
                        save = f"{pref}_{name}.{ext}"
                    else:
                        save = f"{base}_{name}"
            print(f"Plotting '{name}' ({i+1}/{len(names_2d)})")
            try:
                func(kernel, save=save)
            except Exception as e:
                print(f"Plot error for '{name}':", e)

    if args.heatmap and not args.plot:
        _apply_to_all_2d(plot_heatmap)
        return
    if args.box and not args.plot:
        # pass global cols and show-fliers into each call
        def _boxwrap(k, save=None):
            plot_boxplot(k, cols=getattr(args, 'cols_list', None), showfliers=args.show_fliers, save=save)
        _apply_to_all_2d(_boxwrap)
        return
    if args.colhist and not args.plot:
        def _colwrap(k, save=None):
            plot_column_histograms(k, cols=getattr(args, 'cols_list', None), max_cols=args.max_cols, save=save)
        _apply_to_all_2d(_colwrap)
        return

    # histogram
    if args.hist:
        plot_hist(list(inits.values()), save=args.save)
        return

    # kernel plot
    if args.plot:
        if args.plot not in inits:
            print("Initializer not found. Use --list.")
            return
        kernel = inits[args.plot]
        # prefer specific 2D visuals when requested
        try:
            if args.colhist:
                plot_column_histograms(kernel, save=args.save)
                return
            if args.box:
                plot_boxplot(kernel, save=args.save)
                return
            if args.heatmap:
                plot_heatmap(kernel, save=args.save)
                return
        except ValueError as e:
            print("Plot error:", e)
            return

        # default: visualise kernel (single 2D slice for conv/kernel weights)
        plot_kernel(kernel, save=args.save)
        return

    # line & scatter
    if args.line or args.scatter:
        arr = np.concatenate([w.ravel() for w in inits.values()]) if inits else np.array([])
        if args.line:
            plot_line(arr, save=args.save)
        if args.scatter:
            plot_scatter(arr, save=args.save)
        return

    print("No option selected. Use --list, --hist, --plot NAME, --line, --scatter or --compare.")


if __name__ == "__main__":
    main()
# ...existing code...