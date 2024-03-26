# cd Apps.AmazonS3

FILE="Apps.AmazonS3.csproj"  # Specify the path to your XML file

# Read the current version
current_version=$(sed -n 's|.*<Version>\(.*\)</Version>.*|\1|p' $FILE)

# Increment the version
# Assuming the version format is Major.Minor.Patch, we'll increment the Patch number
IFS='.' read -ra ADDR <<< "$current_version"
major=${ADDR[0]}
minor=${ADDR[1]}
patch=${ADDR[2]}
let "patch+=1"

# Use sed to replace the version in the file
sed -i '' "s|<Version>$current_version</Version>|<Version>$major.$minor.$patch</Version>|g" $FILE

dotnet publish --configuration Release
(cd bin/Release/net8.0/publish && zip -r ../../../../../blackbird_app_sp.zip . -x "*.DS_Store")
