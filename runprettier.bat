@echo off

pushd "%~dp0"

call npm install --save-dev prettier @prettier/plugin-xml

call npx prettier ^
  --plugin=@prettier/plugin-xml ^
  --write "**/*.xaml" "**/*.md" ^
  --end-of-line crlf ^
  --bracket-same-line true ^
  --single-attribute-per-line true ^
  --xml-quote-attributes double ^
  --xml-self-closing-space true ^
  --xml-whitespace-sensitivity ignore ^
  --prose-wrap always

pause

popd
