# Biologic Repository Management

## Repository Structure

`Biologic` is maintained as a **separate Git repository** independent from `dcs-all-parent`.

### Repository Details

- **Location**: `C:\Users\h20309\Desktop\repo\dcs-all-parent\Biologic`
- **Branch**: `Biologic` (independent from parent repository)
- **Remote**: Not yet configured (local repository only)

### Current Status

? **Biologic is a standalone Git repository**
- Has its own `.git` directory
- Has its own commit history
- Is NOT a submodule of dcs-all-parent
- Is ignored by dcs-all-parent's `.gitignore`

## Working with Biologic

### Committing Changes to Biologic

```bash
cd Biologic
git add .
git commit -m "Your commit message"
```

### Viewing Biologic History

```bash
cd Biologic
git log
```

### Creating Remote Repository (Future)

When you have permissions to create a remote repository:

```bash
# 1. Create dcs-biologic repository in Azure DevOps
# 2. Add remote
cd Biologic
git remote add origin https://dev.azure.com/horibadevops/HOR-Lab-Automation-Project/_git/dcs-biologic

# 3. Push to remote
git push -u origin Biologic
```

## Integration with dcs-all-parent

### Option 1: Keep as Separate Repository (Current)

**Pros**:
- ? Independent version control
- ? Can have different branching strategy
- ? Clear separation of concerns
- ? Easier to share Biologic code with other projects

**Cons**:
- ? Not automatically cloned with dcs-all-parent
- ? Need to manually manage Biologic updates

**Setup**:
```bash
# Clone dcs-all-parent
git clone https://dev.azure.com/.../dcs-all-parent.git
cd dcs-all-parent

# Clone Biologic separately
git clone https://dev.azure.com/.../dcs-biologic.git Biologic
```

### Option 2: Convert to Git Submodule (Future)

**Pros**:
- ? Automatically cloned with parent
- ? Version locked to specific commit
- ? Standard Git workflow

**Cons**:
- ? More complex to manage
- ? Requires remote repository first

**Setup** (when remote exists):
```bash
cd dcs-all-parent
git submodule add https://dev.azure.com/.../dcs-biologic.git Biologic
git commit -m "Add Biologic as submodule"
```

## Current Commit History

```
f18eabe (HEAD -> Biologic) Complete Biologic electrochemistry system implementation
088b77d Add MethodParameters for OPC UA Method support
0175789 (origin/master, origin/HEAD, master) Update the InfoModel for Ink Measurement System.
```

## Next Steps

1. **Request Azure DevOps repository creation**
   - Repository name: `dcs-biologic`
   - Project: HOR-Lab-Automation-Project

2. **Push to remote**
   ```bash
   cd Biologic
   git remote add origin https://dev.azure.com/.../dcs-biologic.git
   git push -u origin Biologic
   ```

3. **Update dcs-all-parent** (choose one):
   - Keep as separate clone (manual management)
   - Convert to submodule (automatic tracking)

## Important Notes

?? **Do NOT run these commands in Biologic folder**:
- `rm -rf .git` - This will delete the Biologic repository!
- `git init` - Biologic already has a repository

? **The Biologic repository is preserved and active**
- All your commits are safe
- Git history is intact
- Ready to push to remote when available
