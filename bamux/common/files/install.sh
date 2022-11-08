cd ~/
mkdir -p ./.bam/scripts
cd ./.bam/scripts

npm install @bryanapellanes/bam.js@latest
npm update @bryanapellanes/bam.js
cd node_modules/@bryanapellanes/bam.js/
node install-toolkit.js
rm -fr ~/.bam/scripts/node_modules/@bryanapellanes/bam.js
