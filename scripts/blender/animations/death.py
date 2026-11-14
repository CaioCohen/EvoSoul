"""Virgin Drop Death: slow loss of support, slump, then a low grey puddle.

Only shape-key coordinates change. The body maps preserve ordered horizontal
slices: positive lateral factors and strictly increasing height on z in [0, 1].
The height-dependent widening/shear makes this a flowing deformation rather
than uniform scaling. Basis, topology, materials and object transforms remain
owned by common. Facial marks follow the slump, then retract inside the puddle.
The final pose is held on frames 32..36 (30 fps, total duration 1.2 seconds).
"""

from pathlib import Path
import sys

_DIRECTORY = str(Path(__file__).resolve().parent)
if _DIRECTORY not in sys.path:
    sys.path.insert(0, _DIRECTORY)
import common


def _soften(co, vertex_index):
    """Lower the crown most, widen the belly and let the crown sag forward."""
    x, y, z = co
    return (
        x * (1.22 - 0.10 * z),
        y * (1.28 - 0.12 * z) - 0.10 * z * z,
        z * (0.72 - 0.22 * z),
    )


def _puddle(co, vertex_index):
    """Spread to a broad asymmetrical puddle, retaining 0.038 m top height."""
    x, y, z = co
    if vertex_index >= 194:
        # Keep the existing charcoal faces as tiny internal patches, above the
        # floor and beneath the central body surface; no topology/material edit.
        return (0.08 * x, 0.08 * y, 0.012 + 0.003 * z)
    return (
        x * (1.95 - 0.28 * z),
        y * (1.78 - 0.20 * z) - 0.06 * z,
        0.038 * z * (1.40 - 0.40 * z),
    )


def build():
    """Create the independent non-looping Death action via the shared helper."""
    poses = {"Death_Soften": _soften, "Death_Puddle": _puddle}
    # Matching knots and complementary values after the slump avoid summing
    # two full displacements. AUTO_CLAMPED also keeps the final plateau static.
    tracks = {
        "Death_Soften": [
            (0, 0.0), (8, 0.22), (16, 1.0), (23, 0.42),
            (32, 0.0), (36, 0.0),
        ],
        "Death_Puddle": [
            (0, 0.0), (8, 0.0), (16, 0.0), (23, 0.58),
            (32, 1.0), (36, 1.0),
        ],
    }
    action = common.build_clip("Death", 36, poses, tracks)
    print("Death: 0..36 at 30 fps; slump at 16; 0.038 m puddle held 32..36")
    return action


if __name__ == "__main__":
    build()
