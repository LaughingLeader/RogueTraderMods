import os
from pathlib import Path
import zipfile
import argparse
import shutil
import dataclasses
import json
from dataclass_wizard import JSONWizard

script_dir = Path(os.path.dirname(os.path.abspath(__file__)))
os.chdir(script_dir)

repo_user = "LaughingLeader"
repo_name = "RogueTraderMods"

publish_dir = script_dir.joinpath("publish")
latest_dir = publish_dir.joinpath("latest")

parser = argparse.ArgumentParser()
parser.add_argument("-d", "--directory", type=str, required=True, help="The file directory to zip")
parser.add_argument("-v", "--version", type=str, required=True, help="The project version")

args = parser.parse_args()

release_dir:Path = Path(args.directory.replace('"', ""))
release_version:str = args.version

project_name = release_dir.name
zip_name = f"{project_name}_v{release_version}.zip"
release_zip_file = publish_dir.joinpath(zip_name)
release_dir_str = str(release_dir)

download_url = f"https://github.com/{repo_user}/{repo_name}/releases/download/v{release_version}-{project_name}/{zip_name}"

release_zip_file.parent.mkdir(parents=True, exist_ok=True)

ziph = zipfile.ZipFile(release_zip_file, 'w')
for root, dirs, files in os.walk(release_dir):
	for file in files:
		ziph.write(os.path.join(root, file), arcname=os.path.join(root.replace(release_dir_str, project_name), file), compress_type=zipfile.ZIP_DEFLATED)
ziph.close()

latest_dir.mkdir(parents=True, exist_ok=True)
shutil.copyfile(str(release_zip_file), str(latest_dir.joinpath(f"{project_name}.zip")))

print(f"Zipped project: {release_zip_file}")

@dataclasses.dataclass
class RepositoryReleaseEntry:
    Id:str
    Version:str
    DownloadUrl:str

@dataclasses.dataclass
class RepositoryData(JSONWizard):
    Releases:list[RepositoryReleaseEntry] = dataclasses.field(default_factory=list)

    def get_entry(self, name:str, version:str)->RepositoryReleaseEntry|None:
        for entry in self.Releases:
            if entry.Id == name and entry.Version == version:
                return entry

releases_json = script_dir.joinpath("Repository.json")
if releases_json.exists():
	data = json.loads(releases_json.read_text(encoding="utf-8"))
	releases_data = RepositoryData.from_dict(data)
	needs_save = False
	entry = releases_data.get_entry(project_name, release_version)
	if not entry:
		releases_data.Releases.append(RepositoryReleaseEntry(project_name, release_version, download_url))
		needs_save = True
	releases_data.Releases.sort(key=lambda entry: (entry.Id, entry.Version))
	releases_json.write_text(json.dumps(dataclasses.asdict(releases_data), indent="\t"), encoding="utf-8")