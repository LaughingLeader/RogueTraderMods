import os
from pathlib import Path
import zipfile
import argparse
import shutil

script_dir = Path(os.path.dirname(os.path.abspath(__file__)))
os.chdir(script_dir)

publish_dir = script_dir.joinpath("publish")
latest_dir = publish_dir.joinpath("latest")

parser = argparse.ArgumentParser()
parser.add_argument("-d", "--directory", type=str, required=True, help="The file directory to zip")
parser.add_argument("-v", "--version", type=str, required=True, help="The project version")

args = parser.parse_args()

release_dir:Path = Path(args.directory.replace('"', ""))
release_version:str = args.version

print(release_dir)

project_name = release_dir.name
release_zip_file = publish_dir.joinpath(f"{project_name}_v{release_version}.zip")
release_dir_str = str(release_dir)

release_zip_file.parent.mkdir(parents=True, exist_ok=True)

ziph = zipfile.ZipFile(release_zip_file, 'w')
for root, dirs, files in os.walk(release_dir):
	for file in files:
		ziph.write(os.path.join(root, file), arcname=os.path.join(root.replace(release_dir_str, project_name), file), compress_type=zipfile.ZIP_DEFLATED)
ziph.close()

latest_dir.mkdir(parents=True, exist_ok=True)
shutil.copyfile(str(release_zip_file), str(latest_dir.joinpath(f"{project_name}.zip")))

print(f"Zipped project: {release_zip_file}")