# Windows MSIX Trust Status Smoke Plan

**Goal:** Add read-only signer trust-store status to the signed MSIX smoke helper.

## Steps

- [x] Add a failing packaging asset test for trust-store status output.
- [x] Add `Get-SignerTrustStatus` to `smoke-msix-install.ps1`.
- [x] Print `Trust store status` and `Read-only trust check` in the default smoke plan.
- [x] Keep certificate import/trust/install behavior unchanged and gated.
- [x] Update packaging spec and project completion docs.
- [ ] Run focused packaging tests, full solution tests, Debug x64 build, and whitespace check.
- [ ] Commit the completed slice and request review.

## Verification

- Red focused packaging test failed because `Get-SignerTrustStatus` was absent.
- Green focused packaging test passed: 1 test.

## References

- Microsoft MSIX signing and trust guidance.
- Existing `smoke-msix-install.ps1` safety gate for Authenticode `Valid` status before `Add-AppxPackage`.
