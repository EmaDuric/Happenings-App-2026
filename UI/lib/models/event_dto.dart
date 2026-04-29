class EventDto {
  final int id;
  final String name;
  final String description;
  final DateTime eventDate;
  final String? locationName;
  final String? categoryName;
  final String? imageUrl;
  final int? locationId; // ← DODAJ
  final int? eventCategoryId; // ← DODAJ

  EventDto({
    required this.id,
    required this.name,
    required this.description,
    required this.eventDate,
    this.locationName,
    this.categoryName,
    this.imageUrl,
    this.locationId,
    this.eventCategoryId,
  });

  factory EventDto.fromJson(Map<String, dynamic> json) {
    return EventDto(
      id: json['id'] as int,
      name: json['name'] ?? '',
      description: json['description'] ?? '',
      eventDate: DateTime.parse(json['eventDate']),
      locationName: json['locationName'],
      categoryName: json['categoryName'],
      imageUrl: json['imageUrl'],
      locationId: json['locationId'],
      eventCategoryId: json['eventCategoryId'],
    );
  }
}
