{ pkgs ? import <nixpkgs> {
  config.allowUnfree = true;
} }:

with pkgs;

pkgs.mkShell {
  buildInputs = [
    dotnet-sdk_8
    dotnetPackages.Paket
    nodejs_20
    omnisharp-roslyn
  ];
}
