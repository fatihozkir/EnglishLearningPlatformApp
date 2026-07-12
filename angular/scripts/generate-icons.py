from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


output = Path(__file__).resolve().parents[1] / "src" / "assets" / "icons"
output.mkdir(parents=True, exist_ok=True)

for size in (192, 512):
    image = Image.new("RGB", (size, size), "#12304a")
    draw = ImageDraw.Draw(image)
    inset = int(size * 0.08)
    draw.rounded_rectangle(
        (inset, inset, size - inset, size - inset),
        radius=int(size * 0.18),
        fill="#1f6f8b",
    )
    font = ImageFont.load_default(size=int(size * 0.28))
    draw.text((size / 2, size / 2), "EL", anchor="mm", fill="white", font=font)
    image.save(output / f"icon-{size}x{size}.png", optimize=True)
