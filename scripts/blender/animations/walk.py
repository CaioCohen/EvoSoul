"""Grounded in-place Virgin Drop crawl: 21 frames at 30 fps (0.7 s).

Reach toward Blender -Y, then gather the rear. Continuous spatial fields
carry facial marks with the body, keep ground contact, and preserve Basis.
Neutral endpoint holds give the loop a zero-velocity seam. The shared
helper owns interpolation, scene state and all six shape-key tracks.
"""

from pathlib import Path
import sys

_HERE = str(Path(__file__).resolve().parent)
if _HERE not in sys.path:
    sys.path.insert(0, _HERE)

import common


def _smoothstep(value):
    value = max(0.0, min(1.0, value))
    return value * value * (3.0 - 2.0 * value)


def _stretch(co, vertex_index):
    """Reach with front and upper mass while the rear stays planted."""
    x, y, z = co
    height = max(0.0, min(1.0, z))
    contact = _smoothstep(z / 0.14)
    front = _smoothstep((0.4 - y) / 0.8)
    reach = 0.28 * front * contact * (0.6 + 0.4 * height)
    # Narrow the reaching front; lower the crown instead of making a hop.
    return (
        x * (1.0 - 0.10 * front * contact),
        y - reach,
        z * (1.0 - 0.12 * contact),
    )


def _gather(co, vertex_index):
    """Pull the rear forward with a small lateral compression bulge."""
    x, y, z = co
    height = max(0.0, min(1.0, z))
    contact = _smoothstep(z / 0.14)
    rear = _smoothstep((y + 0.4) / 0.8)
    pull = 0.20 * rear * contact * (1.0 - 0.35 * height)
    return (
        x * (1.0 + 0.10 * rear * contact),
        y - pull,
        z * (1.0 - 0.045 * contact),
    )


def build():
    """Create only Walk poses/action through the shared clip builder."""
    # Matching knot times keep stretch-to-gather weights complementary.
    # Neutral holds force flat AUTO_CLAMPED tangents at the loop seam.
    action = common.build_clip(
        "Walk",
        21,
        {"Walk_Stretch": _stretch, "Walk_Gather": _gather},
        {
            "Walk_Stretch": [(0, 0), (1, 0), (7, 1), (14, 0), (20, 0), (21, 0)],
            "Walk_Gather": [(0, 0), (1, 0), (7, 0), (14, 1), (20, 0), (21, 0)],
        },
    )
    print("Built Walk: frames 0..21, 0.7 s; grounded reach / rear gather; neutral loop.")
    return action


if __name__ == "__main__":
    build()
