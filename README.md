<div align="center">

# RP Checker <br/> ![GitHub](https://img.shields.io/github/license/vcb-s/rp-checker) [![Build status](https://ci.appveyor.com/api/projects/status/1uifyy3wsi8fblts?svg=true&passingText=%E7%BC%96%E8%AF%91%20-%20%E7%A8%B3%20&pendingText=%E5%B0%8F%E5%9C%9F%E8%B1%86%E7%82%B8%E4%BA%86%20&failingText=%E6%88%91%E6%84%9F%E8%A7%89%E5%8D%9C%E8%A1%8C%20)](https://ci.appveyor.com/project/tautcony/rp-checker) ![GitHub all releases](https://img.shields.io/github/downloads/vcb-s/rp-checker/total)

A simple tool to check if a video file is corrupted by calculating [PSNR](https://en.wikipedia.org/wiki/Peak_signal-to-noise_ratio)/[SSIM](https://en.wikipedia.org/wiki/Structural_similarity)/[GMSD](https://www4.comp.polyu.edu.hk/~cslzhang/IQA/GMSD/GMSD.htm) with another reference video file.

</div>


## Instructions

- You must have the .NET Framework 4.8 available from Windows Update.

- VapourSynth script version provide PSNR/GMSD metrics, it has ability to handle different resolution video file and yeild more accurate result, custom script template(with `.vpy` extension) is also available in this version.

- FFmpeg is an alternative processor providing both PSNR/SSIM metrics with faster speed, you can get ffmpeg from [here](https://ffmpeg.org/).

- You can switch these options by clicking the form icon at the top left corner, GMSD option is available in a combo box at the right panel.

- You need to install [VapourSynth R54](https://github.com/vapoursynth/vapoursynth/releases) or higher with the corresponding [python version](https://www.python.org/downloads/) to enable VapourSynth processor.

- PSNR(VS) requires [L-SMASH](https://github.com/AkarinVS/L-SMASH-Works) and [vs-ComparePlane](https://github.com/AmusementClub/vs-ComparePlane) to be installed.

- GMSD(VS) requires [L-SMASH](https://github.com/AkarinVS/L-SMASH-Works) and [muvsfunc](https://github.com/WolframRhodium/muvsfunc) and its corresponding dependency libraries: [mvsfunc](https://github.com/HomeOfVapourSynthEvolution/mvsfunc/releases), [havsfunc](https://github.com/HomeOfVapourSynthEvolution/havsfunc), [fmtconv](https://github.com/EleonoreMizo/fmtconv) to be installed.


## Thanks to

- vpy script from [nmm-hd](https://www.nmm-hd.org/newbbs/viewtopic.php?f=23&t=1813)
- [EFS](https://github.com/amefs) for improvement of adaptive resolution compare.
- [New RPC template](https://github.com/AmusementClub/vapoursynth-script/blob/master/RpcTemplate.vpy)


## Source Code

- [Github](https://github.org/vcb-s/rp-checker)
