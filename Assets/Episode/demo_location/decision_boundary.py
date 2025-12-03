import argparse
from pathlib import Path
import numpy as np
import matplotlib.pyplot as plt

def heuristic_policy(obs):
    # obs: [pos_x, pos_y, vel_x, vel_y, angle, ang_vel, leg_right, leg_left]
    x, y = obs[0], obs[1]
    # Simple decision rules to produce a demo decision boundary similar to common examples
    if x > 0.2:
        return 1  # right engine
    if x < -0.2:
        return 3  # left engine
    if y < -0.2:
        return 2  # main (down) engine
    return 0  # no engine


def generate_grid_actions(policy_fn, xlim, ylim, nx, ny, fixed_obs):
    xs = np.linspace(xlim[0], xlim[1], nx)
    ys = np.linspace(ylim[0], ylim[1], ny)
    actions = np.zeros((ny, nx), dtype=int)
    for i, yy in enumerate(ys[::-1]):
        for j, xx in enumerate(xs):
            obs = np.array([xx, yy, *fixed_obs], dtype=float)
            # policy_fn returns an integer action
            a = policy_fn(obs)
            actions[i, j] = int(a)
    return xs, ys, actions


def plot_decision_map(xs, ys, actions, out_file=None, title='Decision Boundary'):
    # actions: shape (ny, nx)
    cmap_colors = {
        0: '#1f77b4',  # blue - no engine
        1: '#d62728',  # red - right engine
        2: '#e377c2',  # pink - down/main engine
        3: '#17becf',  # cyan - left engine
    }
    # build discrete colormap
    from matplotlib.colors import ListedColormap
    cmap = ListedColormap([cmap_colors[i] for i in sorted(cmap_colors.keys())])

    fig, ax = plt.subplots(figsize=(6, 5))
    extent = [xs[0], xs[-1], ys[0], ys[-1]]
    ax.imshow(actions, origin='lower', extent=extent, cmap=cmap, interpolation='nearest', aspect='auto')
    ax.set_xlabel('position x')
    ax.set_ylabel('position y')
    ax.set_title(title)

    # Legend patches
    import matplotlib.patches as mpatches
    patches = [mpatches.Patch(color=cmap_colors[k], label=lab) for k, lab in
               zip([0,1,2,3], ['no engine', 'right engine', 'down engine', 'left engine'])]
    ax.legend(handles=patches, loc='upper right')

    if out_file:
        fig.tight_layout()
        fig.savefig(out_file, dpi=150)
        print(f"Saved decision map to: {out_file}")
    else:
        plt.show()


def main():
    p = argparse.ArgumentParser(description='Decision boundary plot for LunarLander-style policy')
    p.add_argument('--model', type=str, default=None, help='Path to a PyTorch model (optional)')
    p.add_argument('--out', type=str, default='decision_map.png', help='Output image file')
    p.add_argument('--nx', type=int, default=300, help='Grid resolution in x')
    p.add_argument('--ny', type=int, default=240, help='Grid resolution in y')
    p.add_argument('--xlim', type=float, nargs=2, default=[-1.0, 1.0], help='x range')
    p.add_argument('--ylim', type=float, nargs=2, default=[-0.5, 1.5], help='y range')
    p.add_argument('--fixed', type=float, nargs=6, default=[0,0,0,0,0,0],
                   help='Fixed values for [vel_x, vel_y, angle, ang_vel, right_contact, left_contact]')
    args = p.parse_args()

    model_path = Path(args.model) if args.model else None

    policy_fn = heuristic_policy
    # If a model is provided try to load as a PyTorch model
    if model_path and model_path.exists():
        try:
            import torch
            loaded = torch.load(str(model_path), map_location='cpu')
            if isinstance(loaded, torch.nn.Module):
                net = loaded
            else:
                # Try to infer: if it's a state_dict, create a simple MLP with matching size
                # Fallback: wrap loaded if callable
                if callable(loaded):
                    net = loaded
                else:
                    print('Loaded object is not a nn.Module; falling back to heuristic policy')
                    net = None
        except Exception as e:
            print('Failed to load model (PyTorch):', e)
            net = None

        if 'net' in locals() and net is not None:
            def net_policy(obs):
                import torch
                obs_t = torch.tensor(obs, dtype=torch.float32).unsqueeze(0)
                with torch.no_grad():
                    out = net(obs_t)
                # Expect output either Q-values or action logits
                if out.ndim == 2 and out.shape[1] >= 1:
                    act = int(out.argmax(dim=1).item())
                else:
                    act = int(out.item())
                return act

            policy_fn = net_policy
    else:
        if args.model:
            print(f"Model path provided but not found: {args.model}. Using heuristic demo policy.")

    xs, ys, actions = generate_grid_actions(policy_fn, args.xlim, args.ylim, args.nx, args.ny, args.fixed)

    out_file = Path(args.out)
    plot_decision_map(xs, ys, actions, out_file=out_file)


if __name__ == '__main__':
    main()
