﻿module ARCtrl.FileSystem.DefaultGitignore

let dgi = """# ----- macos rules -----
# taken from https://github.com/github/gitignore/blob/main/Global/macOS.gitignore
# General
.DS_Store
.AppleDouble
.LSOverride
# Icon must end with two \r
Icon
# Thumbnails
._*
# Files that might appear in the root of a volume
.DocumentRevisions-V100
.fseventsd
.Spotlight-V100
.TemporaryItems
.Trashes
.VolumeIcon.icns
.com.apple.timemachine.donotpresent
# Directories potentially created on remote AFP share
.AppleDB
.AppleDesktop
Network Trash Folder
Temporary Items
.apdisk
# ----- windows rules -----
# taken from https://github.com/github/gitignore/blob/main/Global/Windows.gitignore
# Windows thumbnail cache files
Thumbs.db
Thumbs.db:encryptable
ehthumbs.db
ehthumbs_vista.db
# Dump file
*.stackdump
# Folder config file
[Dd]esktop.ini
# Recycle Bin used on file shares
$RECYCLE.BIN/
# Windows Installer files
*.cab
*.msi
*.msix
*.msm
*.msp
# Windows shortcuts
*.lnk
# ----- linux rules -----
# taken from https://github.com/github/gitignore/blob/main/Global/Linux.gitignore
*~
# temporary files which can be created if a process still has a handle open of a deleted file
.fuse_hidden*
# KDE directory preferences
.directory
# Linux trash folder which might appear on any partition or disk
.Trash-*
# .nfs files are created when an open file is removed but is still being accessed
.nfs*
"""