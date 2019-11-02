#!/usr/bin/env perl
use File::Basename;
our $^I = ".bak";

my $dirname = dirname(__FILE__);
opendir DIR, $dirname or die $!;
(my $projectName = (grep { /\.sln$/ } readdir DIR)[0]) =~ s/(.+)\.sln$/$1/g;
closedir DIR or die $!;

my $version = @ARGV[0];
my $message = "Bumped version to $version";
my $info = "$dirname/$projectName/Properties/AssemblyInfo.cs";

`git flow release start $version`;
our @ARGV = ($info);
while (<>) {
  s/^\[assembly: AssemblyVersion\("([\d\.]+)"\)\]/\[assembly: AssemblyVersion\("$version"\)\]/g;
  s/^\[assembly: AssemblyFileVersion\("([\d\.]+)"\)\]/\[assembly: AssemblyFileVersion\("$version"\)\]/g;
  print;
}
`git add $info` and `rm $info.bak`;

`git commit -m "$message"`;
`git tag -a $version -m "$message"` or `git tag -d $version`;
`git flow release finish $version -m "$message"`;
