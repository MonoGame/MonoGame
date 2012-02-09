#!/usr/bin/perl -w

use warnings;
use strict;
use Digest::SHA1;
use Cwd;

use FindBin qw($Bin);
my $testdir = $Bin;
undef $Bin;
#print("testdir is $testdir\n");
my $binpath = getcwd();
#print("binpath is $binpath\n");

my $GPrintCmds = 0;

my @modules = qw( preprocessor assembler compiler parser );


sub compare_files {
    my ($a, $b, $endlines) = @_;

    if (not open(FILE1, '<', $a)) {
        return (0, "Couldn't open '$a' for checksum");
    }
    if (not open(FILE2, '<', $b)) {
        close(FILE1);
        return (0, "Couldn't open '$b' for checksum");
    }

    my $sha1 = Digest::SHA1->new;
    my $sha2 = Digest::SHA1->new;

    if (not $endlines) {
        $sha1->addfile(*FILE1);
        $sha2->addfile(*FILE2);
    } else {
        while (<FILE1>) { s/[\r\n]//g; $sha1->add($_); }
        while (<FILE2>) { s/[\r\n]//g; $sha2->add($_); }
    }

    close(FILE1);
    close(FILE2);

    if ($sha1->hexdigest ne $sha2->hexdigest) {
        return (0, "Result doesn't match expectations");
    }

    return (1);
}

my %tests = ();

$tests{'output'} = sub {
    my ($module, $fname) = @_;
    my $output = 'unittest_tempoutput';
    my $desired = $fname . '.correct';
    my $cmd = undef;
    my $endlines = 1;

    # !!! FIXME: this should go elsewhere.
    if ($module eq 'preprocessor') {
        $cmd = "$binpath/mojoshader-compiler -P '$fname' -o '$output'";
    } else {
        return (0, "Don't know how to do this module type");
    }
    $cmd .= ' 2>/dev/null 1>/dev/null';

    print("$cmd\n") if ($GPrintCmds);

    if (system($cmd) != 0) {
        unlink($output) if (-f $output);
        return (0, "External program reported error");
    }

    if (not -f $output) { return (0, "Didn't get any output file"); }

    my @retval = compare_files($desired, $output, $endlines);
    unlink($output);
    return @retval;
};

$tests{'errors'} = sub {
    my ($module, $fname) = @_;
    my $error_output = 'unittest_temperroutput';
    my $output = 'unittest_tempoutput';
    my $desired = $fname . '.correct';
    my $cmd = undef;
    my $endlines = 1;

    # !!! FIXME: this should go elsewhere.
    if ($module eq 'preprocessor') {
        $cmd = "$binpath/mojoshader-compiler -P '$fname' -o '$output'";
    } else {
        return (0, "Don't know how to do this module type");
    }
    $cmd .= " 2>$error_output 1>/dev/null";

    print("$cmd\n") if ($GPrintCmds);

    system($cmd);
    unlink($output) if (-f $output);

    if (not -f $error_output) { return (0, "Didn't get any error output"); }

    my @retval = compare_files($desired, $error_output, $endlines);
    unlink($error_output);
    return @retval;
};

my $totaltests = 0;
my $pass = 0;
my $fail = 0;
my $skip = 0;
my @fails = ();

my $result = '';
chdir($testdir) or die("Failed to chdir('$testdir'): $!\n");
foreach (@modules) {
    my $module = $_;
    foreach (keys %tests) {
        my $testtype = $_;
        my $fn = $tests{$_};
        my $d = "$module/$testtype";
        next if (not -d $d);  # no tests at the moment.
        opendir(TESTDIR, $d) || die("Failed to open dir '$d': $!\n");
        my $subsection = " ... $module / $testtype ...\n";
        print($subsection);
        my $addedsubsection = 0;
        my $fname = readdir(TESTDIR);
        while (defined $fname) {
            my $isfail = 0;
            my $origfname = $fname;
            $fname = readdir(TESTDIR);  # set for next iteration.
            next if (-d $origfname);
            next if ($origfname =~ /\.correct\Z/);
            my $fullfname = "$d/$origfname";
            my ($rc, $reason) = &$fn($module, $fullfname);
            if ($rc == 1) {
                $result = 'PASS';
                $pass++;
            } elsif ($rc == 0) {
                $isfail = 1;
                $result = 'FAIL';
                $fail++;
            } elsif ($rc == -1) {
                $result = 'SKIP';
                $skip++;
            }

            if (defined $reason) {
                $reason = " ($reason)";
            } else {
                $reason = '';
            }
            my $output = "$result ${origfname}${reason}\n";
            print($output);

            if ($isfail) {
                if (!$addedsubsection) {
                    $addedsubsection = 1;
                    push(@fails, $subsection);
                }
                push(@fails, $output);
            }

            $totaltests++;
        }
        closedir(TESTDIR);
    }
}

if (scalar(@fails)) {
    print("\n\n");
    print("*************************************************************\n");
    print("*************************************************************\n");
    print("** SOME TESTS FAILED! PLEASE CORRECT THE FOLLOWING ISSUES. **\n");
    print("*************************************************************\n");
    print("*************************************************************\n");
    print("\n");
    foreach (@fails) {
        print $_;
    }
    print("\n\n");
}

print("\n$totaltests tests, $pass passed, $fail failed, $skip skipped.\n\n");

exit(($fail > 0) ? 1 : 0);

# end if run_tests.pl ...

