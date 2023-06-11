using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Update;
using Streetcode.BLL.DTO.Analytics;
using Streetcode.BLL.DTO.Analytics.Update;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.DTO.Partners.Update;
using Streetcode.BLL.DTO.Streetcode.RelatedFigure;
using Streetcode.BLL.DTO.Streetcode.Update;
using Streetcode.BLL.DTO.Streetcode.Update.Interfaces;
using Streetcode.BLL.DTO.Timeline.Update;
using Streetcode.BLL.DTO.Toponyms;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Entities.Analytics;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Entities.Toponyms;
using Streetcode.DAL.Repositories.Interfaces.Base;
using RelatedFigureModel = Streetcode.DAL.Entities.Streetcode.RelatedFigure;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.Update
{
    internal class UpdateStreetcodeHandler : IRequestHandler<UpdateStreetcodeCommand, Result<StreetcodeUpdateDTO>>
	{
		private readonly IMapper _mapper;
		private readonly IRepositoryWrapper _repositoryWrapper;

		public UpdateStreetcodeHandler(IMapper mapper, IRepositoryWrapper repositoryWrapper)
		{
			_mapper = mapper;
			_repositoryWrapper = repositoryWrapper;
		}

		public async Task<Result<StreetcodeUpdateDTO>> Handle(UpdateStreetcodeCommand request, CancellationToken cancellationToken)
		{
            var streetcodeToUpdate = _mapper.Map<StreetcodeContent>(request.Streetcode);

            await UpdateTimelineItemsAsync(streetcodeToUpdate, request.Streetcode.TimelineItems);
            /*await UpdateStreetcodeArtsAsync(streetcodeToUpdate, request.Streetcode.StreetcodeArts);*/

            _repositoryWrapper.StreetcodeRepository.Update(streetcodeToUpdate);
            /*UpdateStreetcodeToponym(request.Streetcode.StreetcodeToponym);*/
            await UpdateRelatedFiguresRelationAsync(request.Streetcode.RelatedFigures);
            await UpdatePartnersRelationAsync(request.Streetcode.Partners);
            await UpdateStatisticRecords(request.Streetcode.StatisticRecords);
            // await UpdateCoordinates(request.Streetcode.StreetcodeCoordinates);
            _repositoryWrapper.SaveChanges();

            return await GetOld(streetcodeToUpdate.Id);
		}

		private async Task UpdateStreetcodeArtsAsync(StreetcodeContent streetcode, IEnumerable<StreetcodeArtUpdateDTO> arts)
        {
            var (toUpdate, toCreate, toDelete) = CategorizeItems<StreetcodeArtUpdateDTO>(arts);

            var artsToCreate = new List<StreetcodeArt>();

            foreach(var art in toCreate)
            {
                var newArt = _mapper.Map<StreetcodeArt>(art);
                newArt.Art.Image = await _repositoryWrapper.ImageRepository.GetFirstOrDefaultAsync(x => x.Id == art.Art.ImageId);
                newArt.Art.Image.Alt = art.Art.Title;
                artsToCreate.Add(newArt);
            }

            /*var artsToUpdate = new List<StreetcodeArt>();

            foreach(var art in toUpdate)
            {
                var newArt = _mapper.Map<StreetcodeArt>(art);
                newArt.Art.Image = await _repositoryWrapper.ImageRepository.GetFirstOrDefaultAsync(x => x.Id == art.ImageId);
                newArt.Art.Image.Alt = art.Title;
                artsToUpdate.Add(newArt);
            }*/

            var art2 = _mapper.Map<List<StreetcodeArt>>(toUpdate);
            streetcode.StreetcodeArts.AddRange(artsToCreate);

           /* _repositoryWrapper.StreetcodeArtRepository.DeleteRange(art2);*/
        }

		private async Task UpdateTimelineItemsAsync(StreetcodeContent streetcode, IEnumerable<TimelineItemUpdateDTO> timelineItems)
        {
            var newContexts = timelineItems.SelectMany(x => x.HistoricalContexts).Where(c => c.Id == 0).DistinctBy(x => x.Title);
            var newContextsDb = _mapper.Map<IEnumerable<HistoricalContext>>(newContexts);
            await _repositoryWrapper.HistoricalContextRepository.CreateRangeAsync(newContextsDb);
            await _repositoryWrapper.SaveChangesAsync();

            var (toUpdate, toCreate, toDelete) = CategorizeItems<TimelineItemUpdateDTO>(timelineItems);

            var timelineItemsUpdated = new List<TimelineItem>();
            foreach(var timelineItem in toUpdate)
            {
                timelineItemsUpdated.Add(_mapper.Map<TimelineItem>(timelineItem));
                var (historicalContextToUpdate, historicalContextToCreate, historicalContextToDelete) = CategorizeItems<HistoricalContextUpdateDTO>(timelineItem.HistoricalContexts);

                var deletedItems = historicalContextToDelete.Select(x => new HistoricalContextTimeline
                {
                    TimelineId = timelineItem.Id,
                    HistoricalContextId = x.Id,
                })
                .ToList();

                var createdItems = historicalContextToCreate.Select(x => new HistoricalContextTimeline
                {
                    TimelineId = timelineItem.Id,
                    HistoricalContextId = x.Id == 0
                        ? newContextsDb.FirstOrDefault(x => x.Title.Equals(x.Title)).Id
                        : x.Id
                })
                .ToList();

                _repositoryWrapper.HistoricalContextTimelineRepository.DeleteRange(deletedItems);
                await _repositoryWrapper.HistoricalContextTimelineRepository.CreateRangeAsync(createdItems);
            }

            streetcode.TimelineItems.AddRange(timelineItemsUpdated);

            var timelineItemsCreated = new List<TimelineItem>();
            foreach(var timelineItem in toCreate)
            {
                var timelineItemCreate = _mapper.Map<TimelineItem>(timelineItem);
                timelineItemCreate.HistoricalContextTimelines = timelineItem.HistoricalContexts
                  .Select(x => new HistoricalContextTimeline
                  {
                      HistoricalContextId = x.Id == 0
                          ? newContextsDb.FirstOrDefault(x => x.Title.Equals(x.Title)).Id
                          : x.Id
                  })
                 .ToList();

                timelineItemsCreated.Add(timelineItemCreate);
            }

            streetcode.TimelineItems.AddRange(timelineItemsCreated);

            _repositoryWrapper.TimelineRepository.DeleteRange(_mapper.Map<List<TimelineItem>>(toDelete));
        }

		private async Task UpdateStatisticRecordsWithCoordinates(IEnumerable<>)
        {

        }

		private void AddStatisticRecords(StreetcodeContent streetcode, IEnumerable<StatisticRecordDTO> statisticRecords)
        {
            var statisticRecordsToCreate = new List<StatisticRecord>();

            foreach (var statisticRecord in statisticRecords)
            {
                var newStatistic = _mapper.Map<StatisticRecord>(statisticRecord);

                // newStatistic.StreetcodeCoordinate = streetcode.Coordinates.FirstOrDefault(
                //  x => x.Latitude == newStatistic.StreetcodeCoordinate.Latitude && x.Longtitude == newStatistic.StreetcodeCoordinate.Longtitude);
                statisticRecordsToCreate.Add(newStatistic);
            }

            streetcode.StatisticRecords.AddRange(statisticRecordsToCreate);
        }

		private async Task UpdateStatisticRecords(IEnumerable<StatisticRecordUpdateDTO> statisticRecords)
        {
            var (toUpdate, toCreate, toDelete) = CategorizeItems<StatisticRecordUpdateDTO>(statisticRecords);

            await _repositoryWrapper.StatisticRecordRepository.CreateRangeAsync(_mapper.Map<IEnumerable<StatisticRecord>>(toCreate));
            _repositoryWrapper.StatisticRecordRepository.DeleteRange(_mapper.Map<IEnumerable<StatisticRecord>>(toDelete));
        }

		private void UpdateStreetcodeToponym(IEnumerable<StreetcodeToponymUpdateDTO> streetcodeToponymsDTO)
		{
            var toDelete = streetcodeToponymsDTO.Where(_ => _.ModelState == Enums.ModelState.Deleted);
            var toCreate = streetcodeToponymsDTO.Where(_ => _.ModelState == Enums.ModelState.Created);

            foreach (var streetcodeToponymToDelete in toDelete)
			{
                var streetcodeToponym = _mapper.Map<StreetcodeToponym>(streetcodeToponymToDelete);
                _repositoryWrapper.StreetcodeToponymRepository.Delete(streetcodeToponym);
			}

            foreach (var streetcodeToponymToCreate in toCreate)
			{
                var streetcodeToponym = _mapper.Map<StreetcodeToponym>(streetcodeToponymToCreate);
                _repositoryWrapper.StreetcodeToponymRepository.Create(streetcodeToponym);
			}
		}

		private async Task UpdateRelatedFiguresRelationAsync(IEnumerable<RelatedFigureUpdateDTO> relatedFigureUpdates)
		{
            var (toUpdate, toCreate, toDelete) = CategorizeItems<RelatedFigureUpdateDTO>(relatedFigureUpdates);

            await _repositoryWrapper.RelatedFigureRepository.CreateRangeAsync(_mapper.Map<IEnumerable<RelatedFigureModel>>(toCreate));
            _repositoryWrapper.RelatedFigureRepository.DeleteRange(_mapper.Map<IEnumerable<RelatedFigureModel>>(toDelete));
        }

		private async Task UpdatePartnersRelationAsync(IEnumerable<PartnersUpdateDTO> partnersUpdateDTOs)
        {
            var (toUpdate, toCreate, toDelete) = CategorizeItems<PartnersUpdateDTO>(partnersUpdateDTOs);

            await _repositoryWrapper.PartnerStreetcodeRepository.CreateRangeAsync(_mapper.Map<IEnumerable<StreetcodePartner>>(toCreate));
            _repositoryWrapper.PartnerStreetcodeRepository.DeleteRange(_mapper.Map<IEnumerable<StreetcodePartner>>(toDelete));
        }

		private async Task<StreetcodeUpdateDTO> GetOld(int id)
        {
            var updatedStreetcode = await _repositoryWrapper.StreetcodeRepository.GetFirstOrDefaultAsync(s => s.Id == id, include:
                x => x.Include(s => s.Text)
                .Include(s => s.Subtitles)
                .Include(s => s.TransactionLink)
                .Include(s => s.Toponyms));

            var updatedDTO = _mapper.Map<StreetcodeUpdateDTO>(updatedStreetcode);
            return updatedDTO;
		}

		private (IEnumerable<T> toUpdate, IEnumerable<T> toCreate, IEnumerable<T> toDelete) CategorizeItems<T>(IEnumerable<T> items)
              where T : IModelState
        {
            var toUpdate = new List<T>();
            var toCreate = new List<T>();
            var toDelete = new List<T>();

            foreach (var item in items)
            {
                switch (item.ModelState)
                {
                    case Enums.ModelState.Updated:
                        toUpdate.Add(item);
                        break;
                    case Enums.ModelState.Created:
                        toCreate.Add(item);
                        break;
                    default:
                        toDelete.Add(item);
                        break;
                }
            }

            return (toUpdate, toCreate, toDelete);
        }
    }
}
