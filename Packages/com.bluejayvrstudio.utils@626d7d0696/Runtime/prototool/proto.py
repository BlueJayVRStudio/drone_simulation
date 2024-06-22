import os
import subprocess
import shutil

proto_files = []

def iterate_dir(dir):
    # Directory items
    items = os.listdir(dir)

    # Iterate through items
    for item in items:
        _, file_extension = os.path.splitext(f"{dir}/{item}")
        # Handle proto files
        if file_extension.lower() == '.proto':
            print(f"{dir}/{item}")
            proto_files.append((f"{dir}/{item}", item))
        # Iterate subdirectories
        if os.path.isdir(f"{dir}/{item}"):
            iterate_dir(f"{dir}/{item}")

# Delete previously generated csharp files
if (os.path.isdir(f"gen")):
    shutil.rmtree("gen")
# Delete previously generated intermediate files
if (os.path.isdir(f"intermediate")):
    shutil.rmtree("intermediate")
# Recreate intermediate and gen folders
os.mkdir("gen")
os.mkdir("gen/csharp")
os.mkdir("intermediate")

# Iterate at relative root. If using this as an unmodified part of BlueJayUtils, keep relative path as "../../../"
# This will allow you keep proto files in any location of your Unity project
iterate_dir("../../../")
print(proto_files)

# Copy all proto files into intermediate directory
for proto in proto_files:
    shutil.copy(proto[0], f"intermediate/{proto[1]}")

# Prep cmd's
commands = []
for proto in proto_files:
    commands.append(f"echo starting protoc for {proto[1]}")
    commands.append(f"protoc -I ./intermediate/ --csharp_out=./gen/csharp ./intermediate/{proto[1]}")
    commands.append("echo completed protoc")

# Let it rip
commands = " && ".join(commands)
command = commands.split(" ")
process = subprocess.Popen(command, shell=True)
process.wait()

# Clean up intermediate directory
if (os.path.isdir(f"intermediate")):
    shutil.rmtree("intermediate")