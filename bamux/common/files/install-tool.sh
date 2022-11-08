TOOLNAME=$1

cd ~/
mkdir -p ./.bam/scripts
cd ./.bam/scripts

npm install @bryanapellanes/bam.js@latest
npm update @bryanapellanes/bam.js
cd node_modules/@bryanapellanes/bam.js/
node install-tool.js $TOOLNAME
rm -fr ~/.bam/scripts/node_modules/@bryanapellanes/bam.js