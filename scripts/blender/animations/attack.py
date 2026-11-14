"""Grounded 0.5-second Virgin Drop head-butt, authored as two shape keys.

Blender -Y is forward. The foot stays fixed while a height-dependent shear
winds the upper body backward, snaps it forward, and recoils to neutral.
Body and facial marks use the same continuous deformation. No topology,
Basis coordinates, materials, or object transforms are edited here.
"""

from pathlib import Path
import sys

_MODULE_DIR = str(Path(__file__).resolve().parent)
if _MODULE_DIR not in sys.path:
    sys.path.insert(0, _MODULE_DIR)

import common


def _upper_weight(z):
    """Smooth foot-to-crown influence for the one-metre-tall source mesh."""
    height = min(1.0, max(0.0, z))
    return height * height * (3.0 - 2.0 * height)


def _anticipate(co, vertex_index):
    """Load the strike with a backward lean and gentle upper-body compression."""
    x, y, z = co
    upper = _upper_weight(z)
    return (
        x * (1.0 + 0.06 * upper),
        y * (1.0 - 0.04 * upper) + 0.20 * upper,
        z * (1.0 - 0.08 * upper),
    )


def _lunge(co, vertex_index):
    """Extend the head forward over a planted base without translating the root."""
    x, y, z = co
    upper = _upper_weight(z)
    return (
        x * (1.0 - 0.10 * upper),
        y * (1.0 + 0.12 * upper) - 0.44 * upper,
        z * (1.0 - 0.14 * upper),
    )


def build():
    """Create the Attack KEY action at 30 fps; return common's action result."""
    # Shared transition knots give a complementary full anticipation -> full
    # lunge handoff over frames 4..7. The small later anticipation is recoil.
    # Neutral holds at both ends let AUTO_CLAMPED handles settle smoothly.
    action = common.build_clip(
        "Attack",
        15,
        poses={
            "Attack_Anticipate": _anticipate,
            "Attack_Lunge": _lunge,
        },
        tracks={
            "Attack_Anticipate": [
                (0, 0.0), (1, 0.0), (4, 1.0), (7, 0.0),
                (8, 0.0), (11, 0.15), (14, 0.0), (15, 0.0),
            ],
            "Attack_Lunge": [
                (0, 0.0), (1, 0.0), (4, 0.0), (7, 1.0),
                (8, 1.0), (11, 0.0), (14, 0.0), (15, 0.0),
            ],
        },
    )
    print("Attack: frames 0..15 at 30 fps; anticipation 4, impact 7..8, neutral 14..15")
    return action


if __name__ == "__main__":
    build()
