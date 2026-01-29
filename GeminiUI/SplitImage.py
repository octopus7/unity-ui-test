from PIL import Image
import os

source_path = r"d:\github\unity-ui-test\GeminiUI\Assets\Textures\AnimalsSpriteSheet.png"
output_dir = r"d:\github\unity-ui-test\GeminiUI\Assets\Textures"

img = Image.open(source_path)
width, height = img.size
# 512x512 expected
half_w = width // 2
half_h = height // 2

# Top-Left: Duck
duck = img.crop((0, 0, half_w, half_h))
duck.save(os.path.join(output_dir, "Duck.png"))

# Top-Right: Chicken
chicken = img.crop((half_w, 0, width, half_h))
chicken.save(os.path.join(output_dir, "Chicken.png"))

# Bottom-Left: Goose
goose = img.crop((0, half_h, half_w, height))
goose.save(os.path.join(output_dir, "Goose.png"))

# Bottom-Right: Cow
cow = img.crop((half_w, half_h, width, height))
cow.save(os.path.join(output_dir, "Cow.png"))

print("Splitting complete.")
