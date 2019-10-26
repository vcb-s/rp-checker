# RP Cheker [![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0) [![Build status](https://ci.appveyor.com/api/projects/status/1uifyy3wsi8fblts?svg=true&passingText=%E7%BC%96%E8%AF%91%20-%20%E7%A8%B3%20&pendingText=%E5%B0%8F%E5%9C%9F%E8%B1%86%E7%82%B8%E4%BA%86%20&failingText=%E6%88%91%E6%84%9F%E8%A7%89%E5%8D%9C%E8%A1%8C%20)](https://ci.appveyor.com/project/tautcony/rp-checker)

- A simple tool to check if a video file corrupted by calculate PSNR/SSIM with another reference video file.


## Directions

- You must have .NET Framework 4.8 available from Windows Update.

- FFmpeg is now the default processor with both PSNR/SSIM, you can get ffmpeg from [here](https://ffmpeg.org/).

- VapourSynth script version has ability to handle defferent resolution video file and yeild more correct result, you can even write you own template script for even more complex condition.

- You shall install [VapourSynth R29](https://github.com/vapoursynth/vapoursynth/releases) or higher with [mvsfunc](https://github.com/HomeOfVapourSynthEvolution/mvsfunc/releases) and [python 3.6.0](https://www.python.org/downloads/) or higher to enable this processor.


## Thanks to

- vpy script from [nmm-hd](https://www.nmm-hd.org/newbbs/viewtopic.php?f=23&t=1813)
- [EFS](https://github.com/amefs) for improvement of adaptive resolution compare.
- [mvsfunc](https://github.com/HomeOfVapourSynthEvolution/mvsfunc)


## Source Code

- https://github.org/vcb-s/rp-checker
