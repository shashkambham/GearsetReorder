# I thought I could make the config json for the project
# the same as the repo json, only to realize that the repo
# json is a list of config jsons. It's easier to just deal with
# this inline rather than getting people to change the repo json,
# and I've decided to be lazy today.
mv GearsetReorder.json tmp.json
cat tmp.json | jq '.[0]' > GearsetReorder.json
dotnet publish GearsetReorder.csproj
rm GearsetReorder.json
mv tmp.json GearsetReorder.json
echo "Release at $(realpath bin/Release/GearsetReorder/latest.zip)"
start bin\\Release\\GearsetReorder
