---
description: Generate cute 4x4 sprite sheet and slice it into individual assets
---

This workflow generates a 1024x1024 sprite sheet of cute/pastel game items and uses a Python script to slice them into transparent sprites.

PREREQUISITES:
- Python 3.11+ installed
- `rembg` and `Pillow` installed: `pip install rembg[cpu] Pillow`
- The `slice_sprites.py` script must exist in the artifacts or project directory.

1. Generate the Sprite Sheet
   - Use the `generate_image` tool with the following prompt:
     "A sprite sheet of 16 high-quality cute game items arranged in a 4x4 grid. The style should be soft, pastel, feminine, and casual, suitable for a cozy UI. NO PIXEL ART. Hand-drawn or high-res digital art style with soft outlines.
      Row 1: Red Potion, Blue Potion, Green Potion, Purple Elixir
      Row 2: Iron Sword, Wooden Bow, Magic Staff, Dagger
      Row 3: Leather Helmet, Iron Shield, Wizard Hat, Boots
      Row 4: Gold Coin, Old Key, Treasure Map, Mysterious Book
      The background must be a solid white color to allow easy background removal. There should be no grid lines, frames, or borders between items. Each item should be centered in its virtual cell."
   - Save the image to the current workspace.

2. Run the Slicing Script
   - Execute the Python script to slice and remove background.
   - Note: Update `SOURCE_IMAGE_PATH` in the script if the generated image name changes.
   
   ```powershell
   # Example command (adjust path as needed)
   $env:NUMBA_CACHE_DIR = ".numba_cache"; python slice_sprites.py
   ```

3. Verify Assets
   - Check `Assets/Textures/Items` for the 16 new sprite files.
