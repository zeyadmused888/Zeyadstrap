import re
import shutil
from pathlib import Path

exports = Path(input("Path of folder of exported Crowdin files: ").strip().strip('"'))
dest = Path(input("Destination resources folder: ").strip().strip('"'))

icu_codes = {
    "zh-CN": "zh-Hans-CN",
    "zh-HK": "zh-Hant-HK",
    "zh-TW": "zh-Hant-TW",
}

dest.mkdir(parents=True, exist_ok=True)

for filename in exports.rglob("Strings.*"):
    if not filename.is_file():
        continue

    locale = None
    for parent in filename.parents:
        if parent == exports:
            break

        if re.fullmatch(r"[A-Za-z]{2,3}(?:-[A-Za-z0-9]{2,4})*", parent.name):
            locale = parent.name
            break

    if locale is None:
        print(f"Skipping {filename} (could not detect locale folder)")
        continue

    locale = icu_codes.get(locale, locale)
    target = dest / f"Strings.{locale}.resx"
    print(f"Copying {filename} -> {target}")
    shutil.copyfile(filename, target)
