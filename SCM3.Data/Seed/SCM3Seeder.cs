using Microsoft.EntityFrameworkCore;
using SCM3.Data.Entities;

namespace SCM3.Data.Seed;

// Demo dataset described in root CLAUDE.md §2 — ships with the SQLite db so the app
// is usable out of the box without a domain controller or SQL Server.
public static class SCM3Seeder
{
    public static async Task SeedAsync(DbContext.SCM3DbContext context)
    {
        if (await context.Customers.AnyAsync())
        {
            return;
        }

        // --- Customers / Products / Systems ---------------------------------

        var armyCustomer = new Customer { Name = "US Army" };
        var navyCustomer = new Customer { Name = "US Navy" };
        var usafCustomer = new Customer { Name = "USAF" };
        context.Customers.AddRange(armyCustomer, navyCustomer, usafCustomer);

        var grayEagle = new Product { Name = "Gray Eagle" };
        var predatorB = new Product { Name = "Predator B" };
        var avenger = new Product { Name = "Avenger" };
        context.Products.AddRange(grayEagle, predatorB, avenger);

        await context.SaveChangesAsync();

        var armyGrayEagle = new SystemEntity { Name = "Army Gray Eagle", CustomerId = armyCustomer.CustomerId, ProductId = grayEagle.ProductId };
        var navyPredatorB = new SystemEntity { Name = "Navy Predator B", CustomerId = navyCustomer.CustomerId, ProductId = predatorB.ProductId };
        var usafAvenger = new SystemEntity { Name = "USAF Avenger", CustomerId = usafCustomer.CustomerId, ProductId = avenger.ProductId };
        context.Systems.AddRange(armyGrayEagle, navyPredatorB, usafAvenger);

        // --- Demo login accounts (root CLAUDE.md §3 — mock login, no domain controller) ---

        var viewer = new User { Username = "user", FullName = "Demo Viewer", Email = "user@ga-asi.com", Department = "Stakeholders", Role = "Viewer", Theme = "scm3-gasi" };
        var scmStaff = new User { Username = "scm", FullName = "Demo SCM Staff", Email = "scm@ga-asi.com", Department = "SCM", Role = "SCM_Staff", Theme = "scm3-gasi" };
        var scmAdmin = new User { Username = "scmadmin", FullName = "Demo SCM Admin", Email = "scmadmin@ga-asi.com", Department = "SCM", Role = "SCM_Admin", Theme = "scm3-gasi-dark" };
        var teUser = new User { Username = "teuser", FullName = "Demo TE User", Email = "teuser@ga-asi.com", Department = "Test Engineering", Role = "TE_User", Theme = "scm3-gasi" };
        var eeUser = new User { Username = "eeuser", FullName = "Demo EE User", Email = "eeuser@ga-asi.com", Department = "Electrical Engineering", Role = "EE_User", Theme = "scm3-gasi" };
        context.Users.AddRange(viewer, scmStaff, scmAdmin, teUser, eeUser);

        // NOTE: root CLAUDE.md §2 says "6 Request Types + 8 Statuses", but only 7 types
        // (§5 Type Overview) and 6 statuses (§6 State Machine) are concretely defined
        // anywhere in the plan — seeding exactly those rather than inventing two more of
        // each (tracked as part of Open Item §17.1/§17.7).
        var typeSystem = new RequestType { Name = "System" };
        var typeSegment = new RequestType { Name = "Segment" };
        var typeCsciGaasi = new RequestType { Name = "CSCI GA-ASI" };
        var typeCsciSupplier = new RequestType { Name = "CSCI Supplier" };
        var typeEe = new RequestType { Name = "EE Request" };
        var typeTe = new RequestType { Name = "TE Request" };
        var typeThirdParty = new RequestType { Name = "Third Party SW" };
        context.RequestTypes.AddRange(typeSystem, typeSegment, typeCsciGaasi, typeCsciSupplier, typeEe, typeTe, typeThirdParty);

        var statusDraft = new RequestStatus { Name = "Draft" };
        var statusPending = new RequestStatus { Name = "Pending" };
        var statusInReview = new RequestStatus { Name = "In Review" };
        var statusReleased = new RequestStatus { Name = "Released" };
        var statusRejected = new RequestStatus { Name = "Rejected" };
        var statusTerminated = new RequestStatus { Name = "Terminated" };
        context.RequestStatuses.AddRange(statusDraft, statusPending, statusInReview, statusReleased, statusRejected, statusTerminated);

        await context.SaveChangesAsync();

        // --- Sample requests — one per type, mirroring the hierarchy in root CLAUDE.md §2 ---

        var systemRequest = new Request
        {
            Title = "Army Gray Eagle System",
            Version = "v4.2",
            RequestTypeId = typeSystem.RequestTypeId,
            RequestStatusId = statusReleased.RequestStatusId,
            SystemId = armyGrayEagle.SystemId,
            RequestorUserId = scmAdmin.UserId,
            RequestorName = scmAdmin.FullName,
            RequestorEmail = scmAdmin.Email,
            RequestorPhone = "(858) 555-1201",
            ChargeNumber = "CN-2026-101",
            Priority = "High",
            SubmittedDate = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            ReadyDate = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc),
            NeedDate = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            ReleaseDate = new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            ReleaseDescription = "v4.2 system release incorporating Ground Control Segment and Comms Segment updates.",
            NotesToScm = "Coordinate release timing with the Comms Segment team before final SVM publication."
        };
        context.Requests.Add(systemRequest);
        await context.SaveChangesAsync();

        var segmentRequest = new Request
        {
            Title = "Ground Control Segment",
            Version = "v1.3",
            RequestTypeId = typeSegment.RequestTypeId,
            RequestStatusId = statusInReview.RequestStatusId,
            SystemId = armyGrayEagle.SystemId,
            ParentRequestId = systemRequest.RequestId,
            ParentVersion = systemRequest.Version,
            RequestorUserId = scmStaff.UserId,
            RequestorName = scmStaff.FullName,
            RequestorEmail = scmStaff.Email,
            RequestorPhone = "(858) 555-1202",
            ChargeNumber = "CN-2026-102",
            Priority = "Medium",
            SubmittedDate = new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc),
            ReadyDate = new DateTime(2026, 4, 25, 0, 0, 0, DateTimeKind.Utc),
            NeedDate = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc),
            ReleaseDescription = "Ground Control Segment v1.3 — adds updated flight control and engine controller CSCI releases.",
            NotesToScm = "Waiting on the associated CSCI releases to complete before this can proceed to release."
        };
        context.Requests.Add(segmentRequest);
        await context.SaveChangesAsync();

        var csciGaasiRequest = new Request
        {
            Title = "Flight Control Software",
            Version = "4.2.1",
            RequestTypeId = typeCsciGaasi.RequestTypeId,
            RequestStatusId = statusPending.RequestStatusId,
            SystemId = armyGrayEagle.SystemId,
            ParentRequestId = segmentRequest.RequestId,
            ParentVersion = segmentRequest.Version,
            RequestorUserId = scmStaff.UserId,
            RequestorName = scmStaff.FullName,
            RequestorEmail = scmStaff.Email,
            RequestorPhone = "(858) 555-1203",
            ChargeNumber = "CN-2026-103",
            Priority = "High",
            SubmittedDate = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            ReadyDate = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc),
            NeedDate = new DateTime(2026, 6, 5, 0, 0, 0, DateTimeKind.Utc),
            ReleaseDescription = "Flight Control Software 4.2.1 — adds updated navigation library and PTP synchronization fixes.",
            NotesToScm = "Build verified on the x86_Release target; ready for SCM pickup."
        };
        context.Requests.Add(csciGaasiRequest);
        await context.SaveChangesAsync();

        var csciSupplierRequest = new Request
        {
            Title = "Engine Controller Firmware",
            Version = "2.0.0",
            RequestTypeId = typeCsciSupplier.RequestTypeId,
            RequestStatusId = statusPending.RequestStatusId,
            SystemId = armyGrayEagle.SystemId,
            ParentRequestId = segmentRequest.RequestId,
            ParentVersion = segmentRequest.Version,
            RequestorUserId = scmAdmin.UserId,
            RequestorName = scmAdmin.FullName,
            RequestorEmail = scmAdmin.Email,
            RequestorPhone = "(858) 555-1204",
            ChargeNumber = "CN-2026-104",
            Priority = "Medium",
            SubmittedDate = new DateTime(2026, 5, 12, 0, 0, 0, DateTimeKind.Utc),
            ReadyDate = new DateTime(2026, 5, 22, 0, 0, 0, DateTimeKind.Utc),
            NeedDate = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc),
            ReleaseDescription = "Engine Controller Firmware 2.0.0 — supplier release from Acme Avionics Inc., pending checksum verification.",
            NotesToScm = "Awaiting binary checksum confirmation from Acme Avionics before pickup."
        };
        context.Requests.Add(csciSupplierRequest);
        await context.SaveChangesAsync();

        var eeRequest = new Request
        {
            Title = "Power Distribution Board Assembly Rev C",
            RequestTypeId = typeEe.RequestTypeId,
            RequestStatusId = statusInReview.RequestStatusId,
            RequestorUserId = eeUser.UserId,
            RequestorName = eeUser.FullName,
            RequestorEmail = eeUser.Email,
            RequestorPhone = "(858) 555-1205",
            ChargeNumber = "CN-2026-105",
            Priority = "Medium",
            SubmittedDate = new DateTime(2026, 5, 18, 0, 0, 0, DateTimeKind.Utc),
            ReadyDate = new DateTime(2026, 5, 25, 0, 0, 0, DateTimeKind.Utc),
            NeedDate = new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc),
            ReleaseDescription = "Power Distribution Board Assembly Rev C — incorporates updated connector pinout per ECO-2024-045.",
            NotesToScm = "Assembly drawings attached; release location confirmed with SCM."
        };
        context.Requests.Add(eeRequest);
        await context.SaveChangesAsync();

        var teRequest = new Request
        {
            Title = "PTP Based Rack Computer Test",
            RequestTypeId = typeTe.RequestTypeId,
            RequestStatusId = statusPending.RequestStatusId,
            RequestorUserId = teUser.UserId,
            RequestorName = teUser.FullName,
            RequestorEmail = teUser.Email,
            RequestorPhone = "520-555-0140",
            ChargeNumber = "CN-2026-106",
            Priority = "Low",
            SubmittedDate = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc),
            ReadyDate = new DateTime(2026, 5, 25, 0, 0, 0, DateTimeKind.Utc),
            NeedDate = new DateTime(2026, 6, 8, 0, 0, 0, DateTimeKind.Utc),
            NotesToScm = "Test rig reserved for the week of 2026-06-08; submitted for SCM pickup."
        };
        context.Requests.Add(teRequest);
        await context.SaveChangesAsync();

        var thirdPartyRequest = new Request
        {
            Title = "VxWorks 7.0",
            RequestTypeId = typeThirdParty.RequestTypeId,
            RequestStatusId = statusReleased.RequestStatusId,
            RequestorUserId = scmAdmin.UserId,
            RequestorName = scmAdmin.FullName,
            RequestorEmail = scmAdmin.Email,
            RequestorPhone = "(858) 555-1206",
            ChargeNumber = "CN-2026-107",
            Priority = "Medium",
            SubmittedDate = new DateTime(2026, 3, 22, 0, 0, 0, DateTimeKind.Utc),
            ReadyDate = new DateTime(2026, 3, 28, 0, 0, 0, DateTimeKind.Utc),
            NeedDate = new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc),
            ReleaseDate = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            ReleaseDescription = "VxWorks 7.0.0.14 — approved for use as the flight control computer RTOS; export compliance cleared (EAR99).",
            NotesToScm = "Export compliance review (EAR99) closed out — cleared for release."
        };
        context.Requests.Add(thirdPartyRequest);
        await context.SaveChangesAsync();

        // --- Custom attributes — JSON shapes matching the strongly-typed per-type DTOs
        // in SCM3.Data/Entities/Attributes (root CLAUDE.md §5 — Per-Type UI Field
        // Reference). TE Request keeps its existing shape — that type is explicitly
        // out of scope for the new field reference (Open Item §17.1).

        context.RequestAttributes.AddRange(
            new RequestAttributes
            {
                RequestId = systemRequest.RequestId,
                CustomAttributes = """
                {
                  "AssociatedSegments": [
                    { "SegmentName": "Ground Control Segment", "Version": "v1.3", "State": "In Review" },
                    { "SegmentName": "Comms Segment", "Version": "v1.0", "State": "Released" }
                  ],
                  "AssociatedItemSegments": [
                    { "SegmentName": "Comms Segment", "Version": "v1.0" }
                  ],
                  "CompatibleSoftware": [
                    { "Name": "VxWorks", "Version": "7.0.0.14" },
                    { "Name": "LibNav", "Version": "2.1" }
                  ]
                }
                """
            },
            new RequestAttributes
            {
                RequestId = segmentRequest.RequestId,
                CustomAttributes = """
                {
                  "AssociatedCscis": [
                    { "CsciName": "Flight Control Software", "Version": "4.2.1", "State": "Pending" },
                    { "CsciName": "Engine Controller Firmware", "Version": "2.0.0", "State": "Pending" }
                  ]
                }
                """
            },
            new RequestAttributes
            {
                RequestId = csciGaasiRequest.RequestId,
                CustomAttributes = """
                {
                  "CsciOnlyRelease": false,
                  "Dcns": [
                    { "DcnNumber": "DCN-1023", "Title": "Flight Control Software 4.2.1 Release", "Status": "Approved" }
                  ],
                  "CommonLibraries": [
                    { "LibraryName": "LibNav", "Version": "2.1" }
                  ],
                  "BuildTargets": [
                    { "BuildTarget": "x86_Release", "Platform": "VxWorks 7.0" }
                  ],
                  "LruPartNumbers": [
                    { "PartNumber": "LRU-FCS-001", "Description": "Flight Control Computer LRU" }
                  ],
                  "AssociatedThirdPartySoftware": [
                    { "Vendor": "Wind River Systems", "Product": "VxWorks", "Version": "7.0.0.14" }
                  ]
                }
                """
            },
            new RequestAttributes
            {
                RequestId = csciSupplierRequest.RequestId,
                CustomAttributes = """
                {
                  "SoftwarePointOfContact": "Kevin Chan",
                  "Supplier": "Acme Avionics Inc.",
                  "SupplierPointOfContact": "Bob Johnson",
                  "SupplierPocPhone": "480-555-0200",
                  "SupplierPocEmail": "bjohnson@acmeavionics.com",
                  "Location": "\\\\fileserver\\releases\\supplier",
                  "Dcns": [
                    { "DcnNumber": "DCN-2001", "Title": "Engine Controller Firmware 2.0.0 Release", "Status": "Pending" }
                  ],
                  "BinaryChecksums": [
                    { "Checksum": "a3f1c9d2e8b7456...", "Algorithm": "SHA256" }
                  ],
                  "LrePartNumbers": [
                    { "PartNumber": "LRE-ENG-002", "Description": "Engine Controller Firmware Release Element" }
                  ]
                }
                """
            },
            new RequestAttributes
            {
                RequestId = eeRequest.RequestId,
                CustomAttributes = """
                {
                  "PartNumber": "PDB-REV-C-001",
                  "AssemblyPartNumber": "ASSY-PDB-001",
                  "AssemblyDescription": "Power Distribution Board Assembly Rev C",
                  "DataRights": "UNLIMITED",
                  "ChangeNotice": "ECO-2024-045",
                  "ReleaseLocation": "\\\\fileserver\\EE\\releases"
                }
                """
            },
            new RequestAttributes
            {
                RequestId = teRequest.RequestId,
                CustomAttributes = """
                {
                  "Summary": "Verify PTP synchronization across the rack computer cluster",
                  "ContactPerson": "Demo TE User",
                  "Email": "teuser@ga-asi.com",
                  "Phone": "520-555-0140",
                  "WorkOrder": "WO-2026-3301",
                  "RackComputer": "Rack-14-Node-2",
                  "Operation": "PTP Sync Validation",
                  "Type": "PTP Based"
                }
                """
            },
            new RequestAttributes
            {
                RequestId = thirdPartyRequest.RequestId,
                CustomAttributes = """
                {
                  "Vendor": "Wind River Systems",
                  "ProductName": "VxWorks",
                  "ProductVersion": "7.0.0.14",
                  "CountryOfOrigin": "USA",
                  "Source": "Commercial Off-The-Shelf",
                  "ProductUrl": "https://www.windriver.com/products/vxworks",
                  "SourceLocation": "\\\\fileserver\\thirdparty\\vxworks\\src",
                  "InternalLocation": "\\\\fileserver\\thirdparty\\vxworks\\7.0.0.14",
                  "DownloadFileNames": "vxworks-7.0.0.14-release.iso",
                  "DownloadLocation": "https://docs.windriver.com/downloads/vxworks7",
                  "IntendedUse": "Real-time operating system for the flight control computer",
                  "Platform": "x86_64",
                  "PointOfContact": "Kevin Chan",
                  "BinarySourceOrBoth": "Binary",
                  "LicenseType": "Commercial",
                  "ProductLicense": "Wind River Master Software License Agreement",
                  "ContainsOss": true,
                  "Patent": "None identified",
                  "PatentNonAssertNeeded": "No",
                  "PatentNonAssertApproval": "N/A",
                  "SoftwareLicenseAgreement": "Executed 2024-01-15",
                  "LicenseUrl": "https://www.windriver.com/legal/master-agreement",
                  "LicenseTrackingMethod": "SCM DB Manager — Third Party SW register",
                  "OssObligations": "Attribution notices included in the SVD",
                  "ExportCompliance": "EAR99",
                  "ManagerApproval": "Approved",
                  "Approver": "Demo SCM Admin",
                  "ApprovalDate": "2026-03-25T00:00:00Z",
                  "Status": "Released"
                }
                """
            });

        // --- SVM Notes --------------------------------------------------------

        context.RequestNotes.AddRange(
            new RequestNote { RequestId = systemRequest.RequestId, AuthorUserId = scmAdmin.UserId, NoteText = "Released to the field — all segment and CSCI releases verified against SVM-NODE-001." },
            new RequestNote { RequestId = segmentRequest.RequestId, AuthorUserId = scmStaff.UserId, NoteText = "Picked up for review; waiting on the associated CSCI releases to complete first." },
            new RequestNote { RequestId = csciGaasiRequest.RequestId, AuthorUserId = scmStaff.UserId, NoteText = "Build targets confirmed with the flight software team — ready for SCM pickup." },
            new RequestNote { RequestId = csciSupplierRequest.RequestId, AuthorUserId = scmAdmin.UserId, NoteText = "Awaiting binary checksum confirmation from Acme Avionics before pickup." },
            new RequestNote { RequestId = eeRequest.RequestId, AuthorUserId = eeUser.UserId, NoteText = "Assembly drawings attached; release location confirmed with SCM." },
            new RequestNote { RequestId = teRequest.RequestId, AuthorUserId = teUser.UserId, NoteText = "Test rig reserved for the week of 2026-06-08; submitted for SCM pickup." },
            new RequestNote { RequestId = thirdPartyRequest.RequestId, AuthorUserId = scmAdmin.UserId, NoteText = "Export compliance review (EAR99) closed out — cleared for release." });

        // --- History — transition chain matching each request's current status -

        AddHistoryChain(context, systemRequest, statusDraft, statusPending, statusInReview, statusReleased, scmAdmin, scmStaff, scmAdmin, daysAgo: 60);
        AddHistoryChain(context, segmentRequest, statusDraft, statusPending, statusInReview, null, scmStaff, scmStaff, null, daysAgo: 45);
        AddHistoryChain(context, csciGaasiRequest, statusDraft, statusPending, null, null, scmStaff, null, null, daysAgo: 25);
        AddHistoryChain(context, csciSupplierRequest, statusDraft, statusPending, null, null, scmAdmin, null, null, daysAgo: 23);
        AddHistoryChain(context, eeRequest, statusDraft, statusPending, statusInReview, null, eeUser, scmStaff, null, daysAgo: 17);
        AddHistoryChain(context, teRequest, statusDraft, statusPending, null, null, teUser, null, null, daysAgo: 16);
        AddHistoryChain(context, thirdPartyRequest, statusDraft, statusPending, statusInReview, statusReleased, scmAdmin, scmStaff, scmAdmin, daysAgo: 75);

        await context.SaveChangesAsync();
    }

    // Builds a Draft -> Pending [-> In Review [-> Released]] chain (root CLAUDE.md §6
    // Valid Transitions), stopping at whichever status the sample request currently has.
    private static void AddHistoryChain(
        DbContext.SCM3DbContext context,
        Request request,
        RequestStatus draft,
        RequestStatus pending,
        RequestStatus? inReview,
        RequestStatus? released,
        User requestor,
        User? pickedUpBy,
        User? releasedBy,
        int daysAgo)
    {
        var when = DateTime.UtcNow.AddDays(-daysAgo);

        context.RequestHistory.Add(new RequestHistory
        {
            RequestId = request.RequestId,
            FromStatusId = draft.RequestStatusId,
            ToStatusId = pending.RequestStatusId,
            Action = "Submit",
            ActionByUserId = requestor.UserId,
            LogDate = when
        });

        if (inReview is not null && pickedUpBy is not null)
        {
            when = when.AddDays(3);
            context.RequestHistory.Add(new RequestHistory
            {
                RequestId = request.RequestId,
                FromStatusId = pending.RequestStatusId,
                ToStatusId = inReview.RequestStatusId,
                Action = "Pick Up",
                ActionByUserId = pickedUpBy.UserId,
                LogDate = when
            });
        }

        if (released is not null && inReview is not null && releasedBy is not null)
        {
            when = when.AddDays(5);
            context.RequestHistory.Add(new RequestHistory
            {
                RequestId = request.RequestId,
                FromStatusId = inReview.RequestStatusId,
                ToStatusId = released.RequestStatusId,
                Action = "Release",
                ActionByUserId = releasedBy.UserId,
                LogDate = when
            });
        }
    }
}
