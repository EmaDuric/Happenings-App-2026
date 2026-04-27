class EventDto {
  final int id;
  final String name;
  final String description;
  final DateTime eventDate;

  EventDto({
    required this.id,
    required this.name,
    required this.description,
    required this.eventDate,
  });

  factory EventDto.fromJson(Map<String, dynamic> json) {
    return EventDto(
      id: json['id'] as int,
      name: json['name'] ?? '',
      description: json['description'] ?? '',
      eventDate: DateTime.parse(json['eventDate']),
    );
  }
}