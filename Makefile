bootstrapper := paket.bootstrapper.exe
bootstrapperpath := c:/Programs/Git/Paket/.paket/
nupkgOutputDir := ./publish

all: clean test

updatepaket:
	@mkdir -p .paket \
	&& cp $(bootstrapperpath)/$(bootstrapper) .paket/$(bootstrapper) \
	&& .paket/$(bootstrapper) prerelease

install: updatepaket
	.paket/paket.exe install

build:
	@dotnet build -nologo -v:q -property:GenerateFullPaths=true

#flags for good development experience with stack trace line number and filenames
test: build
	@dotnet test -v:q -nologo

#clean out bin and obj
clean:
	@find . -type d \( -name "bin" -o -name "obj" \) | xargs rm -rf

#runs paket pack command on paket.template files
publish:
	@.paket/paket.exe pack "$(nupkgOutputDir)" \
	&& echo "Published to $(nupkgOutputDir)"

.PHONY: all updatepaket install build test clean publish
