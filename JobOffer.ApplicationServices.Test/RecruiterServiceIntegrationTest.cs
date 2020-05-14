﻿using JobOffer.ApplicationServices.Constants;
using JobOffer.ApplicationServices.Test.Base;
using JobOffer.DataAccess;
using JobOffer.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JobOffer.ApplicationServices.Test
{
    [TestClass]
    public class RecruiterServiceIntegrationTest : IntegrationTestBase
    {
        private readonly RecruiterService _service;
        private readonly RecruiterRepository _recruiterRepository;
        private readonly CompanyRepository _companyRepository;

        public RecruiterServiceIntegrationTest()
        {
            _recruiterRepository = new RecruiterRepository(_database);
            _companyRepository = new CompanyRepository(_database);
            _service = new RecruiterService(_companyRepository, _recruiterRepository);
        }

        [TestInitialize]
        public void Init()
        {
            SetupIntegrationTest.OneTimeTearDown();
            SetupIntegrationTest.OneTimeSetUp();
        }

        [TestCleanup]
        public void Clean()
        {
            SetupIntegrationTest.OneTimeTearDown();
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public async Task CreateCompany_SaveCompanySuccessfully_WhenCompanyDataIsCorrectAndCompanyDoesNotExists()
        {

            //Arrage
            var company = new Company("Acme", "Software");

            //Act
            await _service.CreateCompanyAsync(company);

            var savedCompany = await _companyRepository.GetCompanyAsync(company.Name, company.Activity);

            //Assert
            Assert.AreEqual(company, savedCompany, "Company was not saved");

        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public async Task CreateCompany_ThrowsError_WhenCompanyAlreadyExists()
        {
            //Arrage
            var company = new Company("Acme", "Software");

            var otherCompany = new Company("Acme", "Software");

            //Act
            await _service.CreateCompanyAsync(company);


            try
            {
                await _service.CreateCompanyAsync(otherCompany);

                //Assert
                Assert.Fail("It should not allow creating a company that already exists");
            }
            catch(InvalidOperationException ex)
            {
                //Assert
                Assert.AreEqual(ServicesErrorMessages.COMPANY_ALREADY_EXISTS, ex.Message);
            }

        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public async Task CreateRecruiter_SaveCruiterSuccessfully_WhenRecruiterDataIsCorrectAndRecruiterNotExists()
        {
            //Arrange
            var recruiter = new Recruiter()
            {
                FirstName = "Patricia",
                LastName = "Maidana",
                IdentityCard = "28123456"
            };

            recruiter.AddClientCompany(new Company("Acme", "Software"));

            recruiter.AddJobHistory(new Job("Accenture", "Sr.Talent Adquision", new DateTime(2015,5,1), true));
            recruiter.AddJobHistory(new Job("Accenture", "Sr.Talent Adquision", new DateTime(2014, 1, 1), false, new DateTime(2015,4,30)));

            recruiter.AddStudy(new Study("UBA", "Lic.Relaciones del Trabajo", StudyStatus.Completed));

            //Act
            await _service.CreateRecruiterAsync(recruiter);

            var savedRecruiter = await _service.GetRecruiterAsync(recruiter);

            //Assert
            Assert.AreEqual(recruiter, savedRecruiter, "Recruiter was not saved");
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public async Task UpdateRecruiter_SaveCruiterSuccessfully_WhenRecruiterDataIsCorrectAndRecruiterExists()
        {
            //Arrange
            var recruiter = new Recruiter()
            {
                FirstName = "Patricia",
                LastName = "Maidana",
                IdentityCard = "28123456"
            };

            recruiter.AddClientCompany(new Company("Acme", "Software"));

            recruiter.AddJobHistory(new Job("Accenture", "Sr.Talent Adquision", new DateTime(2015, 5, 1), true));
            recruiter.AddJobHistory(new Job("Accenture", "Sr.Talent Adquision", new DateTime(2014, 1, 1), false, new DateTime(2015, 4, 30)));

            recruiter.AddStudy(new Study("UBA", "Lic.Relaciones del Trabajo", StudyStatus.Completed));

            await _service.CreateRecruiterAsync(recruiter);

            var savedRecruiter = await _service.GetRecruiterAsync(recruiter);

            //Act

            var jobToModify = savedRecruiter.JobHistory.Where(j => j.CompanyName == "Accenture" && j.From.Date == new DateTime(2014, 1, 1).Date).Single();

            jobToModify.CompanyName = "Globant";

            recruiter.UpdateJobHistory(jobToModify);

            await _service.UpdateRecruiterAsync(recruiter);

            var updatedRecruiter = await _service.GetRecruiterAsync(recruiter);

            //Assert
            Assert.AreEqual("Globant", updatedRecruiter.JobHistory.Single(j => j == jobToModify).CompanyName);
        }

    }
}